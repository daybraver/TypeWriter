using NLog;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using TypeWriter.PubSubEvents;
using TypeWriter.Services;
using XamlPearls.Shortcuts;

namespace TypeWriter.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AppConfigSource _appconfigSource = App.Instance.Container.Resolve<AppConfigSource>();
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private MediaPlayerController _mediaPlayerController = App.Instance.Container.Resolve<MediaPlayerController>();

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            e.Cancel = true; // Prevent the window from closing
            this.Hide(); // Hide the window instead of closing it
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            #region Media

            var model = new HotKeyModel("PauseOrResumeMedia", true, false, false, false, Keys.Space);
            try
            {
                this.RegisterGlobalHotKey(model, (model) => { try { _mediaPlayerController.PauseOrResume(); } catch (Exception ex) { _logger.Warn(ex); } });
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                App.Instance.TrayIcon.ShowBalloonTip(nameof(TypeWriter), $"Failed to register {model.Name}.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
            }

            model = new HotKeyModel("NextMedia", true, false, false, false, Keys.Down);
            try
            {
                this.RegisterGlobalHotKey(model, (model) =>
                {
                    try { _mediaPlayerController.Next(false); } catch (Exception ex) { _logger.Warn(ex); }
                });
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                App.Instance.TrayIcon.ShowBalloonTip(nameof(TypeWriter), $"Failed to register {model.Name}.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
            }

            model = new HotKeyModel("PrevMedia", true, false, false, false, Keys.Up);
            try
            {
                this.RegisterGlobalHotKey(model, (model) =>
                {
                    try { _mediaPlayerController.Previous(); } catch (Exception ex) { _logger.Warn(ex); }
                });
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                App.Instance.TrayIcon.ShowBalloonTip(nameof(TypeWriter), $"Failed to register {model.Name}.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
            }

            model = new HotKeyModel("ForwardMedia", true, false, false, false, Keys.Right);

            try
            {
                this.RegisterGlobalHotKey(model, (model) => { try { _mediaPlayerController.Forward(); } catch (Exception ex) { _logger.Warn(ex); } });
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                App.Instance.TrayIcon.ShowBalloonTip(nameof(TypeWriter), $"Failed to register {model.Name}.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
            }

            model = new HotKeyModel("BackMedia", true, false, false, false, Keys.Left);

            try
            {
                this.RegisterGlobalHotKey(model, (model) => { try { _mediaPlayerController.Back(); } catch (Exception ex) { _logger.Warn(ex); } });
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                App.Instance.TrayIcon.ShowBalloonTip(nameof(TypeWriter), $"Failed to register {model.Name}.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
            }

            model = new HotKeyModel("IncSpeed", true, false, false, false, Keys.Oemplus);
            try
            {
                this.RegisterGlobalHotKey(model, (model) =>
                {
                    try { _mediaPlayerController.IncrementSpeedRatio(_appconfigSource.GetConfig().SpeedRatioIncrement); } catch (Exception ex) { _logger.Warn(ex); }

                });
            }
            catch (Exception ex)
            {
                _logger.Warn( ex);

                App.Instance.TrayIcon.ShowBalloonTip(nameof(TypeWriter), $"Failed to register {model.Name}.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
            }

            model = new HotKeyModel("DecSpeed", true, false, false, false, Keys.OemMinus);

            try

            {
                this.RegisterGlobalHotKey(model, (model) =>
                {
                    try { _mediaPlayerController.DecrementSpeedRatio(_appconfigSource.GetConfig().SpeedRatioDecrement); } catch (Exception ex) { _logger.Warn(ex); }
                });
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                App.Instance.TrayIcon.ShowBalloonTip(nameof(TypeWriter), $"Failed to register {model.Name}.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
            }

            model = new HotKeyModel("RstSpeed", true, false, false, false, Keys.R);

            try
            {
                this.RegisterGlobalHotKey(model, (model) =>
                {
                    try { _mediaPlayerController.ResetSpeedRatio(); } catch (Exception ex) { _logger.Warn(ex); }
                });
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                App.Instance.TrayIcon.ShowBalloonTip(nameof(TypeWriter), $"Failed to register {model.Name}.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
            }

            #endregion Media

            model = new HotKeyModel("PlayWordAudio", true, false, true, false, Keys.Space);
            try
            {
                this.RegisterGlobalHotKey(model, (model) =>
                {
                    App.Instance.Container.Resolve<IEventAggregator>()
                    .GetEvent<PlayWordAudioEvent>().Publish();
                });
            }
            catch (Exception ex)
            {
                _logger.Warn(ex);
                App.Instance.TrayIcon.ShowBalloonTip(nameof(TypeWriter), $"Failed to register {model.Name}.", Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Warning);
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
