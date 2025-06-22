using Hardcodet.Wpf.TaskbarNotification;
using System.Windows;
using TypeWriter.Services;
using TypeWriter.ViewModels;
using TypeWriter.Views;

namespace TypeWriter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static App Instance => (Application.Current as App)!;
        public TaskbarIcon TrayIcon => (TaskbarIcon)(this.FindResource("TaskbarIcon"));

        protected override Window CreateShell()
        {
            return new MainWindow();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Container.GetContainer().Dispose(); // 释放容器资源
        }
       
        protected override void OnStartup(StartupEventArgs e)
        {
            Xceed.Wpf.Toolkit.Licenser.LicenseKey = "WTK46-DFFYR-0R9GW-0R1A";

            base.OnStartup(e);

            var trayIcon = FindResource("TaskbarIcon") as TaskbarIcon; // 必须要实例化一下资源，才能激发托盘图标
            trayIcon!.DataContext = new TaskbarIconViewModel(Container.Resolve<IEventAggregator>(), Container.Resolve<IDialogService>(), Container.Resolve<AppConfigSource>(), Container.Resolve<SentenceSource>(), Container.Resolve<WordSource>());
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            RegisterService(containerRegistry);
            RegisterViewModel(containerRegistry);
            containerRegistry.RegisterDialogWindow<DialogWindowSupportedDragMove>(nameof(DialogWindowSupportedDragMove));
            containerRegistry.RegisterDialog<LearnWordView>("learn_word");
            containerRegistry.RegisterDialog<SentenceBrowserView>("browse_sentenses");
        }

        private void RegisterService(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<AppConfigSource>();
            containerRegistry.RegisterSingleton<SentenceSource>();
            containerRegistry.RegisterSingleton<WordSource>();
            containerRegistry.RegisterSingleton<MediaPlayerController>();
        }

        private void RegisterViewModel(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<SentenceBrowserViewModel>();
            containerRegistry.Register<LearnWordViewModel>();
            containerRegistry.Register<MainWindowViewModel>();
        }
    }
}
