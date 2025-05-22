using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TypeWriter.Views
{
    /// <summary>
    /// Interaction logic for LearnWordView.xaml
    /// </summary>
    public partial class LearnWordView : UserControl
    {
        public LearnWordView()
        {
            InitializeComponent();
        }

        private void TextBox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TextBox.Focus();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox.Focus();
        }
    }
}
