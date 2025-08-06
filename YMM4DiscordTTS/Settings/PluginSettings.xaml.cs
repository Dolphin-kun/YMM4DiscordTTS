using System.Windows.Controls;

namespace YMM4DiscordTTS.Settings
{
    /// <summary>
    /// PluginSettings.xaml の相互作用ロジック
    /// </summary>
    public partial class PluginSettings : UserControl
    {
        public PluginSettings()
        {
            InitializeComponent();

            try
            {
                VersionTextBlock.Text = GetVersion.GetPluginVersion();
            }
            catch
            {
                VersionTextBlock.Text = "取得エラー";
            }
        }
    }
}
