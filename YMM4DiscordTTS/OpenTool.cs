using YMM4DiscordTTS.Services;
using YMM4DiscordTTS.View;
using YMM4DiscordTTS.ViewModel;
using YukkuriMovieMaker.Plugin;

namespace YMM4DiscordTTS
{
    public class OpenTool : IToolPlugin
    {
        public string Name => "YMM4Discord読み上げ";
        public Type ViewModelType => typeof(OpenToolViewModel);
        public Type ViewType => typeof(ToolControl);

        static OpenTool()
        {
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        }

        private static void OnProcessExit(object? sender, EventArgs e)
        {
            AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
            _ = DiscordService.Instance.LeaveVoiceChannelAsync();
            VoiceVoxProcessManager.Instance.StopEngine();
        }
    }
}