using Discord.Interactions;
using System.Windows;
using YMM4DiscordTTS.Models;
using YMM4DiscordTTS.Settings;

namespace YMM4DiscordTTS.Commands
{
    [Group("dictionary", "読み上げ辞書を管理します。")]
    public class Dictionary : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("add", "辞書に新しい単語と読み方を登録します。")]
        public async Task AddDictionary(
            [Summary("before", "登録する単語")]
            string beforeText,
            [Summary("after", "単語の読み方")]
            string afterText)
        {
            if (string.IsNullOrWhiteSpace(beforeText) || string.IsNullOrWhiteSpace(afterText))
            {
                await RespondAsync("単語と読み方を両方入力してください。", ephemeral: true);
                return;
            }

            SetDictionary(beforeText, afterText);

            await RespondAsync($"辞書に「{beforeText}」を「{afterText}」として登録/更新しました。", ephemeral: false);
        }

        public static void SetDictionary(string beforeText, string afterText)
        {
            var settings = TTSSettings.Default;
            Application.Current.Dispatcher.Invoke(() =>
            {
                var existingEntry = settings.DictionaryEntries.FirstOrDefault(e => e.Before.Equals(beforeText, System.StringComparison.OrdinalIgnoreCase));

                if (existingEntry != null)
                {
                    existingEntry.After = afterText;
                }
                else
                {
                    var newEntry = new DictionaryEntry { Before = beforeText, After = afterText };
                    settings.DictionaryEntries.Add(newEntry);
                }
            });

            settings.Save();
        }
    }
}
