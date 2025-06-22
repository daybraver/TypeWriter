using System.IO;
using System.Windows;
using System.Windows.Media;
using TypeWriter.PubSubEvents;
using TypeWriter.Services;

namespace TypeWriter.ViewModels
{
    internal class TaskbarIconViewModel : BindableBase
    {
        private readonly AppConfigSource _appConfigSource;
        private readonly IDialogService _dialogService;
        private readonly IEventAggregator _eventAggregator;
        private readonly SentenceSource _sentenceSource;
        private readonly WordSource _wordSource;
        private string _textFilePath;

        public TaskbarIconViewModel(IEventAggregator eventAggregator, IDialogService dialogService, AppConfigSource appConfigSource, SentenceSource sentenceSource, WordSource wordSource)
        {
            _eventAggregator = eventAggregator;
            _appConfigSource = appConfigSource;
            _sentenceSource = sentenceSource;
            _dialogService = dialogService;
            _wordSource = wordSource;

            ShowPerformancesCommand = new DelegateCommand(() =>
            {
                Application.Current.MainWindow.Hide();
                Application.Current.MainWindow.ShowDialog();
            });
        }

        public ObservableLearnWordOption ObservableLearnWordOption { get; set; }
        public DelegateCommand ShowPerformancesCommand { get; }

        public void AutoHide()
        {
            _appConfigSource.GetConfig().AutoHide = !_appConfigSource.GetConfig().AutoHide;
            _appConfigSource.SaveConfig(_appConfigSource.GetConfig());
        }

        public void BrowseSentenses()
        {
            foreach (var window in App.Instance.Windows)
            {
                var w = window as Window;
                if (w != null && w.DataContext != null && w.DataContext.GetType() == typeof(SentenceBrowserViewModel))
                {
                    w.Show();
                    return;
                }
            }
            _dialogService.Show("browse_sentenses", null, null, nameof(DialogWindowSupportedDragMove));
        }

        public void Exit()
        {
            App.Instance.Shutdown();
        }

        public void ListenWords()
        {
            foreach (var window in App.Instance.Windows)
            {
                var w = window as Window;
                if (w != null && w.DataContext != null && w.DataContext.GetType() == typeof(LearnWordViewModel))
                {
                    w.Show();
                    return;
                }
            }
            _dialogService.Show("learn_word", null, null, nameof(DialogWindowSupportedDragMove));
        }

        public void SelectMedia()
        {
            var w = ShowBackWindow();
            Ookii.Dialogs.Wpf.VistaOpenFileDialog vistaOpenFileDialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            vistaOpenFileDialog.Multiselect = false;
            vistaOpenFileDialog.DefaultExt = "";
            vistaOpenFileDialog.Filter = "Media|*.mp3;*mp4";
            vistaOpenFileDialog.RestoreDirectory = true;
            vistaOpenFileDialog.InitialDirectory = _appConfigSource.GetConfig().DefaultMediaFolderPath;

            if (vistaOpenFileDialog.ShowDialog() == true)
            {
                _eventAggregator.GetEvent<MediaSelectedEvent>().Publish(vistaOpenFileDialog.FileName);
            }
            w.Close();
        }

        public void SelectSentenseSource()
        {
            var w = ShowBackWindow();
            Ookii.Dialogs.Wpf.VistaOpenFileDialog vistaOpenFileDialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            vistaOpenFileDialog.Multiselect = false;
            vistaOpenFileDialog.DefaultExt = "";
            vistaOpenFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            vistaOpenFileDialog.RestoreDirectory = true;
            vistaOpenFileDialog.InitialDirectory = Environment.CurrentDirectory;

            if (vistaOpenFileDialog.ShowDialog() == true)
            {
                _textFilePath = vistaOpenFileDialog.FileName;
                _sentenceSource.LoadText(_textFilePath);
                _eventAggregator.GetEvent<SentensesSourceLoadedEvent>().Publish();
            }
            w.Close();
        }

        public void SelectWordsSource()
        {
            var w = ShowBackWindow();
            Ookii.Dialogs.Wpf.VistaOpenFileDialog vistaOpenFileDialog = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            vistaOpenFileDialog.Multiselect = false;
            vistaOpenFileDialog.DefaultExt = string.Empty;
            vistaOpenFileDialog.Filter = "word files (*.txt)|*.txt|All files (*.*)|*.*";
            vistaOpenFileDialog.RestoreDirectory = true;
            vistaOpenFileDialog.InitialDirectory = Environment.CurrentDirectory;

            if (vistaOpenFileDialog.ShowDialog() == true)
            {
                _wordSource.LoadWords(vistaOpenFileDialog.FileName);
            }
            w.Close();
        }

        // 不先New Window Show，文件选择对话框会闪退。这应该是Hardcodet.NotifyIcon.Wpf的bug.
        private Window ShowBackWindow()
        {
            Window w = new Window
            {
                Width = 0,
                Height = 0,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            w.Show();
            return w;
        }
    }
}
