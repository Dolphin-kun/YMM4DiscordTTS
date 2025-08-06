using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using YMM4DiscordTTS.Settings;
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

        private async void ToolControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"初期化中にエラーが発生しました: {ex.Message}");
                MessageBox.Show(
                    "初期化中にエラーが発生しました。\n" + ex.Message,
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async Task InitializeAsync()
        {
            var parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Title = "YMM4 Discord読み上げ";
            }

            if (TTSSettings.Default.IsCheckVersion && await GetVersion.CheckVersionAsync("YMM4Discord読み上げ"))
            {
                string url =
                    "https://ymm4-info.net/";
                var result = MessageBox.Show(
                    $"新しいバージョンがあります。\n\n最新バージョンを確認しますか？\nOKを押すと配布サイトが開きます。\n{url}",
                    "YMM4エクスプローラープラグイン",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Information);

                if (result == MessageBoxResult.OK)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
            }
        }

        private void TokenPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ToolControlViewModel.Instance.Token = TokenPasswordBox.Password;
        }
    }
}