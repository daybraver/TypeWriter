using System.Windows;
using System.Windows.Input;
using TypeWriter.Services;

namespace TypeWriter
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class DialogWindowSupportedDragMove : Window, IDialogWindow
    {
        public DialogWindowSupportedDragMove()
        {
            InitializeComponent();
            this.ResizeMode = ResizeMode.NoResize;
            this.ShowInTaskbar = false;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.WindowStyle = WindowStyle.None;
            this.Topmost = true;
        }

        public IDialogResult Result { get; set; }

        private void Window_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(App.Instance.Container.Resolve<AppConfigSource>().GetConfig().AutoHide)
            {
                this.Hide();
            }
        }

        private void Window_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
