using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CsvHelper;
using NAudio.Wave;
using Nito.AsyncEx;
using NLog;
using TypeWriter.PubSubEvents;
using TypeWriter.Services;

namespace TypeWriter.ViewModels
{
    internal class LearnWordViewModel : BindableBase, IDialogAware, IDisposable
    {
        private readonly AppConfigSource _appConfigSource;
        private readonly IEventAggregator _eventAggregator;
        private readonly AsyncLock _mutex = new AsyncLock();
        private readonly Dictionary<string, string> _phonetics;
        private readonly Timer _timer;
        private readonly Dictionary<string, (byte[] us, byte[] uk)> _wordAudioCache;
        private readonly WordSource _wordSource;
        private Accent _accent;
        private Color _backColor;
        private int _boxHeight;
        private int _boxWidth;
        private bool _disposed;
        private Color _fontColor;
        private FontFamily _fontFamily;
        private double _fontSize;
        private FontStretch _fontStretch;
        private FontStyle _fontStyle;
        private FontWeight _fontWeight;
        private bool _isPaused;
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private string _word;

        public LearnWordViewModel(WordSource wordSource, AppConfigSource appConfigSource, IEventAggregator eventAggregator)
        {
            NextCommand = new DelegateCommand(Next).Catch((ex) => { _logger.Error(ex); });
            PrevCommand = new DelegateCommand(Previous).Catch((ex) => { _logger.Error(ex); });
            KeyDownCommand = new DelegateCommand<KeyEventArgs>(KeyDown).Catch((ex) => { _logger.Error(ex); });

            _wordSource = wordSource;
            _phonetics = new Dictionary<string, string>();
            _wordAudioCache = new Dictionary<string, (byte[] us, byte[] uk)>();
            _appConfigSource = appConfigSource;
            _eventAggregator = eventAggregator;

            _backColor = _appConfigSource.GetConfig().LearnWordOption.BackColor;
            _boxHeight = _appConfigSource.GetConfig().LearnWordOption.BoxHeight;
            _boxWidth = _appConfigSource.GetConfig().LearnWordOption.BoxWidth;
            _fontColor = _appConfigSource.GetConfig().LearnWordOption.FontInfo.BrushColor.Color;
            _fontSize = _appConfigSource.GetConfig().LearnWordOption.FontInfo.Size;
            _fontFamily = _appConfigSource.GetConfig().LearnWordOption.FontInfo.Family;
            _fontStretch = _appConfigSource.GetConfig().LearnWordOption.FontInfo.Stretch;
            _fontStyle = _appConfigSource.GetConfig().LearnWordOption.FontInfo.Style;
            _fontWeight = _appConfigSource.GetConfig().LearnWordOption.FontInfo.Weight;
            _accent = _appConfigSource.GetConfig().LearnWordOption.Accent;

            _eventAggregator.GetEvent<AppConfigChangedEvent>().Subscribe(ChangeConfigs);

            _eventAggregator.GetEvent<PlayWordAudioEvent>().Subscribe(ToggleWordAudioPlayStatus);

            ReadPhonetic();

            _timer = new Timer(async (state) =>
            {
                try
                {
                    await PlayWordAudio(Word);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                _timer.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
            });
            _timer.Change(TimeSpan.FromSeconds(1), Timeout.InfiniteTimeSpan);
        }

        public event Action<IDialogResult> RequestClose;

        public DelegateCommand<KeyEventArgs> KeyDownCommand { get; set; }

        public DelegateCommand NextCommand { get; set; }

        public string Phonetic
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Word) || !_phonetics.ContainsKey(Word.ToLower()))
                {
                    return string.Empty;
                }
                return _phonetics[Word.ToLower()];
            }
        }

        public DelegateCommand PrevCommand { get; set; }

        DialogCloseListener IDialogAware.RequestClose { get; }

        public string Title => string.Empty;

        public string Word
        {
            get { return _word; }
            set { SetProperty(ref _word, value); }
        }

        public bool CanCloseDialog()
        {
            return false;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _eventAggregator.GetEvent<AppConfigChangedEvent>().Unsubscribe(ChangeConfigs);
                _eventAggregator.GetEvent<PlayWordAudioEvent>().Unsubscribe(ToggleWordAudioPlayStatus);
                _disposed = true;
            }
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        private void ChangeConfigs()
        {
            BackColor = _appConfigSource.GetConfig().LearnWordOption.BackColor;
            BoxHeight = _appConfigSource.GetConfig().LearnWordOption.BoxHeight;
            BoxWidth = _appConfigSource.GetConfig().LearnWordOption.BoxWidth;
            FontColor = _appConfigSource.GetConfig().LearnWordOption.FontInfo.BrushColor.Color;
            FontSize = _appConfigSource.GetConfig().LearnWordOption.FontInfo.Size;
            FontFamily = _appConfigSource.GetConfig().LearnWordOption.FontInfo.Family;
            FontStretch = _appConfigSource.GetConfig().LearnWordOption.FontInfo.Stretch;
            FontStyle = _appConfigSource.GetConfig().LearnWordOption.FontInfo.Style;
            FontWeight = _appConfigSource.GetConfig().LearnWordOption.FontInfo.Weight;
            Accent = _appConfigSource.GetConfig().LearnWordOption.Accent;
        }

        private void KeyDown(KeyEventArgs args)
        {
            if (args != null)
            {
                TextBox textBox = args.Source as TextBox;

                if ((_appConfigSource.GetConfig().AutoHide && args.Key == Key.Space) || args.Key == Key.Escape)
                {
                    var parent = LogicalTreeHelper.GetParent(textBox);
                    while (parent is not Window)
                    {
                        parent = LogicalTreeHelper.GetParent(parent);
                    }
                            (parent as Window)?.Hide();
                    return;
                }

                if (args.Key == Key.Enter)
                {
                    textBox.Clear();
                }
                else if (args.Key == Key.Up)
                {
                    Previous();
                    textBox.Clear();
                }
                else if (args.Key == Key.Down)
                {
                    Next();
                    textBox.Clear();
                }
                else if (args.Key == Key.Left)
                {
                    Accent = Accent.US;
                }
                else if (args.Key == Key.Right)
                {
                    Accent = Accent.UK;
                }
                textBox.Focus();
            }
        }

        private void Next()
        {
            _wordSource.Next();
            Word = _wordSource.Word;
            RaisePropertyChanged(nameof(Phonetic));
        }

        private async Task PlayWordAudio(string word)
        {
            if (string.IsNullOrWhiteSpace(word) || _isPaused)
            {
                return;
            }

            using (await _mutex.LockAsync())
            {
                byte[] audioBytes;
                if (!_wordAudioCache.ContainsKey(word))
                {
                    using HttpClient client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(3);
                    var ukAudio = await client.GetByteArrayAsync($"https://dict.youdao.com/dictvoice?audio={word}&type=1");
                    var usAudio = await client.GetByteArrayAsync($"https://dict.youdao.com/dictvoice?audio={word}&type=2");
                    _wordAudioCache[word] = (usAudio, ukAudio);
                }
                audioBytes = Accent == Accent.US ? _wordAudioCache[word].us : _wordAudioCache[word].uk;
                using (var ms = new MemoryStream(audioBytes))
                using (var media = new StreamMediaFoundationReader(ms))
                using (var waveOut = new WaveOutEvent())
                {
                    waveOut.Init(media);
                    waveOut.Play();
                    while (waveOut.PlaybackState == PlaybackState.Playing)
                    {
                        await Task.Delay(100);
                    }
                }
            }
        }

        private void Previous()
        {
            _wordSource.Previous();
            Word = _wordSource.Word;
            RaisePropertyChanged(nameof(Phonetic));
        }

        private void ReadPhonetic()
        {
            _phonetics.Clear();
            using var reader = new StreamReader("word.csv");
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Read();
            csv.ReadHeader();
            while (csv.Read())
            {
                string word = csv.GetField("vc_vocabulary");
                string phoneticUS = csv.GetField("vc_phonetic_us");
                string phoneticUK = csv.GetField("vc_phonetic_uk");
                _phonetics[word.ToLower()] = phoneticUS + " " + phoneticUK;
            }
        }

        private void ToggleWordAudioPlayStatus()
        {
            _isPaused = !_isPaused;
        }

        #region Properties

        public Accent Accent
        {
            get => _accent;
            set
            {
                SetProperty(ref _accent, value);
            }
        }

        public Color BackColor
        {
            get { return _backColor; }
            set { SetProperty(ref _backColor, value); }
        }

        public int BoxHeight
        {
            get { return _boxHeight; }
            set { SetProperty(ref _boxHeight, value); }
        }

        public int BoxWidth
        {
            get { return _boxWidth; }
            set { SetProperty(ref _boxWidth, value); }
        }

        public Color FontColor
        {
            get => _fontColor;
            set => SetProperty(ref _fontColor, value);
        }

        public FontFamily FontFamily
        {
            get => _fontFamily;
            set => SetProperty(ref _fontFamily, value);
        }

        public double FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        public FontStretch FontStretch
        {
            get => _fontStretch;
            set => SetProperty(ref _fontStretch, value);
        }

        public FontStyle FontStyle
        {
            get => _fontStyle;
            set => SetProperty(ref _fontStyle, value);
        }

        public FontWeight FontWeight
        {
            get => _fontWeight;
            set => SetProperty(ref _fontWeight, value);
        }

        #endregion Properties
    }
}
