using DryIoc.ImTools;
using LibVLCSharp.Shared;
using System.Globalization;
using System.IO;
using System.Windows;
using TypeWriter.PubSubEvents;

namespace TypeWriter.Services
{
    internal class MediaPlayerController : IDisposable
    {
        #region IDisposable

        public virtual void CleanUp()
        {
            _mediaPlayer.EndReached -= _mediaPlayer_EndReached;
            _eventAggregator.GetEvent<MediaSelectedEvent>().Unsubscribe(SelectMedia);
            _mediaPlayer?.Media?.Dispose();
            _mediaPlayer?.Dispose();
            _libVLC?.Dispose();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                CleanUp();
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }

        #endregion IDisposable

        private readonly AppConfigSource _appConfigSource;
        private readonly IEventAggregator _eventAggregator;
        private string _currMediaPath;
        private bool _disposed;
        private string _folder;
        private LibVLC _libVLC;
        private MediaPlayer _mediaPlayer;
        private AudioPlayMode _playMode;

        public MediaPlayerController(AppConfigSource appConfigSource,IEventAggregator eventAggregator)
        {
            Core.Initialize();
            _libVLC = new LibVLC();
            _mediaPlayer = new MediaPlayer(_libVLC);
            _mediaPlayer.EndReached += _mediaPlayer_EndReached;
            _appConfigSource = appConfigSource;
            _eventAggregator = eventAggregator;
            _folder = appConfigSource.GetConfig().DefaultMediaFolderPath;
            _playMode = appConfigSource.GetConfig().PlayMode;
            _eventAggregator.GetEvent<MediaSelectedEvent>().Subscribe(SelectMedia);
        }

        public void Back()
        {
            if (_mediaPlayer.Media != null)
            {
                var time = _mediaPlayer.Time;
                if (time > _appConfigSource.GetConfig().ForwardBackTimeMs)
                {
                    _mediaPlayer.Time = time - _appConfigSource.GetConfig().ForwardBackTimeMs;
                }
                else
                {
                    _mediaPlayer.Time = 0;
                }
            }
        }

        public void DecrementSpeedRatio(double decrement)
        {
            if (_mediaPlayer.Media != null && _mediaPlayer.Rate > Convert.ToSingle(decrement))
            {
                var rate = _mediaPlayer.Rate - Convert.ToSingle(decrement);
                // Ensure the rate does not go below a minimum threshold, e.g., 0.2f
                if (rate >= 0.2f)
                {
                    _mediaPlayer.SetRate(rate);
                }
            }
        }

        public void Forward()
        {
            if (_mediaPlayer.Media != null)
            {
                var time = _mediaPlayer.Time;
                long remaining = _mediaPlayer.Length - time;
                if (remaining > _appConfigSource.GetConfig().ForwardBackTimeMs)
                {
                    _mediaPlayer.Time = time + _appConfigSource.GetConfig().ForwardBackTimeMs;
                }
                else
                {
                    _mediaPlayer.Time = _mediaPlayer.Length;
                }
            }
        }

        public void IncrementSpeedRatio(double increment)
        {
            if (_mediaPlayer.Media != null)
            {
                var rate = _mediaPlayer.Rate + Convert.ToSingle(increment);
                if (rate <= 4.0f) // Assuming 4.0f is the maximum rate allowed
                {
                    _mediaPlayer.SetRate(rate);
                }
            }
        }

        public void Next(bool isMediaAutoEnded)
        {
            if (string.IsNullOrWhiteSpace(_folder) || !Path.Exists(_folder))
            {
                App.Instance.TrayIcon.ShowBalloonTip(nameof(TypeWriter), $"{_folder} doesn't exist.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
                return;
            }

            var files = Directory.GetFiles(_folder, "*.*", SearchOption.TopDirectoryOnly).Where(x => x.EndsWith(".mp3", true, CultureInfo.CurrentCulture) || x.EndsWith(".mp4", true, CultureInfo.CurrentCulture)).Order().ToArray();
            if (files.Length == 0)
            {
                App.Instance.TrayIcon.ShowBalloonTip(nameof(TypeWriter), $"{_folder} is empty.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
                return;
            }

            string next = string.Empty;

            if (string.IsNullOrWhiteSpace(_currMediaPath) || !files.Contains(_currMediaPath))
            {
                next = files[0];
            }
            else
            {
                switch (_playMode)
                {
                    case AudioPlayMode.ListLoop:
                    case AudioPlayMode.OrderPlay:
                        int currIndex = files.IndexOf(item => item == _currMediaPath);
                        next = currIndex == -1 || currIndex >= files.Length - 1 ? files[0] : files[currIndex + 1];
                        break;

                    case AudioPlayMode.SingleLoop:
                        if (isMediaAutoEnded)
                        {
                            next = _currMediaPath;
                        }
                        else
                        {
                            currIndex = files.IndexOf(item => item == _currMediaPath);
                            next = currIndex == -1 || currIndex >= files.Length - 1 ? files[0] : files[currIndex + 1];
                        }
                        break;

                    case AudioPlayMode.RandomPlay:
                        next = files[Random.Shared.Next(0, files.Length)];
                        break;
                }
            }

            _currMediaPath = next;
            _folder = Path.GetDirectoryName(next);
            Play(_currMediaPath);
        }

        public void PauseOrResume()
        {
            if (_mediaPlayer.Media != null)
            {
                if (_mediaPlayer.IsPlaying)
                {
                    _mediaPlayer.SetPause(true);
                }
                else
                {
                    _mediaPlayer.SetPause(false);
                }
            }
        }

        public void Play(string fileName)
        {
            if (string.IsNullOrWhiteSpace(_currMediaPath) || !File.Exists(_currMediaPath))
            {
                App.Instance.TrayIcon.ShowBalloonTip(nameof(TypeWriter), $"{_currMediaPath} doesn't exist.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
                return;
            }
            _mediaPlayer.Stop();
            var old = _mediaPlayer.Media;
            _mediaPlayer.Media = new Media(_libVLC, fileName, FromType.FromPath);
            old?.Dispose();
            _mediaPlayer.Play();
        }

        public void Previous()
        {
            if (string.IsNullOrWhiteSpace(_folder) || !Path.Exists(_folder))
            {
                App.Instance.TrayIcon.ShowBalloonTip(nameof(TypeWriter), $"{_folder} doesn't exist.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
                return;
            }

            var files = Directory.GetFiles(_folder, "*.*", SearchOption.TopDirectoryOnly).Where(x => x.EndsWith(".mp3", true, CultureInfo.CurrentCulture) || x.EndsWith(".mp4", true, CultureInfo.CurrentCulture)).Order().ToArray();
            if (files.Length == 0)
            {
                App.Instance.TrayIcon.ShowBalloonTip(nameof(TypeWriter), $"{_folder} is empty.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
                return;
            }

            string prev = string.Empty;

            if (string.IsNullOrWhiteSpace(_currMediaPath) || !files.Contains(_currMediaPath))
            {
                prev = files[^1];
            }
            else
            {
                switch (_playMode)
                {
                    case AudioPlayMode.ListLoop:
                    case AudioPlayMode.OrderPlay:
                    case AudioPlayMode.SingleLoop:
                        int currIndex = files.IndexOf(item => item == _currMediaPath);
                        if (currIndex == -1)
                        {
                            prev = files[0];
                        }
                        else if (currIndex == 0)
                        {
                            prev = files[^1];
                        }
                        else
                        {
                            prev = files[currIndex - 1];
                        }
                        break;

                    case AudioPlayMode.RandomPlay:
                        prev = files[Random.Shared.Next(0, files.Length)];
                        break;
                }
            }
            _currMediaPath = prev;
            _folder = Path.GetDirectoryName(_currMediaPath);

            Play(_currMediaPath);
        }

        public void ResetSpeedRatio()
        {
            if (_mediaPlayer.Media != null)
            {
                _mediaPlayer.SetRate(1.0f);
            }
        }

        public void SelectMedia(string mediaPath)
        {
            _currMediaPath = mediaPath;
            _folder = Path.GetDirectoryName(mediaPath);
            Play(mediaPath);
        }

        public void SetFolder(string folder)
        {
            _folder = folder;
        }

        private void _mediaPlayer_EndReached(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Next(true);
            });
        }
    }
}
