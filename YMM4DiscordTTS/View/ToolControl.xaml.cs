using System.Windows.Controls;
using YMM4DiscordTTS.ViewModel;

namespace YMM4DiscordTTS.View
{
    public partial class ToolControl : UserControl
    {
        public ToolControl()
        {
            InitializeComponent();

            this.DataContext = ToolViewModel.Instance;
            TokenPasswordBox.Password = ToolViewModel.Instance.Token;
        }

        private void TokenPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            ToolViewModel.Instance.Token = TokenPasswordBox.Password;
        }
    }
}