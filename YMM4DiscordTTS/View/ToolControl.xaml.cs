using System.Windows;
using System.Windows.Controls;
using YMM4DiscordTTS.ViewModel;

namespace YMM4DiscordTTS.View
{
    public partial class ToolControl : UserControl
    {
        public ToolControl()
        {
            InitializeComponent();

            this.DataContext = ToolControlViewModel.Instance;
            TokenPasswordBox.Password = ToolControlViewModel.Instance.Token;

            this.Loaded += ToolControl_Loaded;
        }

        private void ToolControl_Loaded(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this);

            if (parentWindow != null)
            {
                parentWindow.Title = "YMM4 Discord読み上げ";
            }
        }

        private void TokenPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            ToolControlViewModel.Instance.Token = TokenPasswordBox.Password;
        }
    }
}