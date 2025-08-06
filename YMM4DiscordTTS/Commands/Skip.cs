using Discord.Interactions;
using YMM4DiscordTTS.ViewModel;

namespace YMM4DiscordTTS.Commands
{
    public class SkipCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("skip", "現在再生中の音声と、キューに溜まっている読み上げをすべてスキップします")]
        public async Task Skip()
        {
            if(ToolControlViewModel.Instance is not null)
            {
                ToolControlViewModel.Instance.SkipPlayback();
                await RespondAsync("読み上げをスキップしました。", ephemeral: false);
            }
            else
            {
                await RespondAsync("スキップ機能の準備ができていません。", ephemeral: true);
            }
        }
    }
}
