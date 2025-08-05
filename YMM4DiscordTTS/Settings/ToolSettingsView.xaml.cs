using YMM4DiscordTTS.Models;
using System.Windows;
using System.Windows.Controls;

namespace YMM4DiscordTTS.Settings
{
    public partial class ToolSettingsView : UserControl
    {
        public ToolSettingsView()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ToolSettings.Default.DictionaryEntries.Add(new DictionaryEntry { Before = "", After = "" });
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is DictionaryEntry entry)
            {
                ToolSettings.Default.DictionaryEntries.Remove(entry);
            }
        }
    }
}
