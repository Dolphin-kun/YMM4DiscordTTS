using YMM4DiscordTTS.Models;
using System.Windows;
using System.Windows.Controls;

namespace YMM4DiscordTTS.Settings
{
    public partial class TTSSettingsView : UserControl
    {
        public TTSSettingsView()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            TTSSettings.Default.DictionaryEntries.Add(new DictionaryEntry { Before = "", After = "" });
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is DictionaryEntry entry)
            {
                TTSSettings.Default.DictionaryEntries.Remove(entry);
            }
        }
    }
}
