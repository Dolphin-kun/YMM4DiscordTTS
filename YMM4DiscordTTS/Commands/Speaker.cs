using Discord;
using Discord.Interactions;
using YMM4DiscordTTS.Settings;

namespace YMM4DiscordTTS.Commands
{
    [Group("speaker", "読み上げ話者を設定します。")]
    public class SpeakerCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("set", "読み上げ話者を設定します。")]
        public async Task SetSpeaker(
            [Summary("speaker", "設定したい話者を選択してください。")]
            [Autocomplete(typeof(SpeakerAutocompleteHandler))]
            int speakerId)
        {
            SetSpeakerForUser(Context.User.Id, speakerId);

            var speakerName = TTSSettings.Default.AvailableSpeakers.FirstOrDefault(s => s.Id == speakerId)?.Name ?? "不明な話者";
            await RespondAsync($"読み上げ話者を「{speakerName}」に設定しました。", ephemeral: true);
        }

        [SlashCommand("reset", "読み上げ話者をデフォルトに戻します。")]
        public async Task ResetSpeaker()
        {
            ResetSpeakerForUser(Context.User.Id);
            await RespondAsync("読み上げ話者をデフォルトに戻しました。", ephemeral: true);
        }


        public static void SetSpeakerForUser(ulong userId, int speakerId)
        {
            var settings = TTSSettings.Default;
            settings.UserVoiceMappings[userId] = speakerId;
            settings.Save();
        }

        public static void ResetSpeakerForUser(ulong userId)
        {
            var settings = TTSSettings.Default;
            if (settings.UserVoiceMappings.TryRemove(userId, out _))
            {
                settings.Save();
            }
        }

        public static int GetSpeakerForUser(ulong userId)
        {
            if (userId == 0)
            {
                return TTSSettings.Default.SpeakerId;
            }

            var settings = TTSSettings.Default;
            return settings.UserVoiceMappings.TryGetValue(userId, out var speakerId)
                ? speakerId
                : settings.SpeakerId;
        }
    }

    public class SpeakerAutocompleteHandler : AutocompleteHandler
    {
        public override Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            try
            {
                var userInput = autocompleteInteraction.Data.Current.Value?.ToString() ?? string.Empty;
                var allSpeakers = TTSSettings.Default.AvailableSpeakers;
                IEnumerable<AutocompleteResult> suggestions;
                if (!string.IsNullOrEmpty(userInput))
                {
                    suggestions = allSpeakers
                        .Where(s => s.Name.Contains(userInput, StringComparison.OrdinalIgnoreCase))
                        .Select(s => new AutocompleteResult(s.Name, s.Id));
                }
                else
                {
                    suggestions = allSpeakers
                        .Select(s => new AutocompleteResult(s.Name, s.Id));
                }
                return Task.FromResult(AutocompletionResult.FromSuccess(suggestions.Take(25)));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"話者リストの自動補完エラー: {ex.Message}");
                return Task.FromResult(AutocompletionResult.FromError(ex));
            }
        }
    }
}
