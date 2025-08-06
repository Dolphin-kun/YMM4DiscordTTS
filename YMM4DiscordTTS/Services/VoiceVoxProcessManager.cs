using System.Diagnostics;
using System.IO;
using System.Windows;
using YMM4DiscordTTS.Settings;

namespace YMM4DiscordTTS.Services
{
    public class VoiceVoxProcessManager
    {
        public static VoiceVoxProcessManager Instance { get; } = new VoiceVoxProcessManager();

        private VoiceVoxProcessManager() { }
        private Process? _voiceVoxProcess;

        public void StartEngineIfNotRunning()
        {
            var processes = Process.GetProcessesByName("run");
            if (processes.Length != 0)
            {
                _voiceVoxProcess = processes[0];
                return;
            }

            try
            {
                string enginePath = TTSSettings.Default.VoiceVoxPath;
                if (string.IsNullOrEmpty(enginePath) && !File.Exists(enginePath))
                {
                    string startupPath = AppDomain.CurrentDomain.BaseDirectory;
                    string targetDir = @"user\resources\VOICEVOX\";
                    string fullSearchPath = Path.Combine(startupPath, targetDir);

                    if (!Directory.Exists(fullSearchPath))
                    {
                        MessageBox.Show($"VOICEVOXのディレクトリが見つかりませんでした。\n指定されたパスにフォルダが存在するか確認してください。\n\n検索パス: {fullSearchPath}");
                        return;
                    }

                    string[] foundFiles = Directory.GetFiles(fullSearchPath, "run.exe", SearchOption.AllDirectories);

                    if (foundFiles.Length == 0)
                    {
                        MessageBox.Show($"VOICEVOXの実行ファイル(run.exe)が見つかりませんでした。\n検索パス: {fullSearchPath}");
                        return;
                    }

                    enginePath = foundFiles[0];

                    TTSSettings.Default.VoiceVoxPath = enginePath;
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = enginePath,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                _voiceVoxProcess = Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("VoiceVoxエンジンの起動に失敗しました: " + ex.Message);
            }
        }

        public void StopEngine()
        {
            try
            {
                if (_voiceVoxProcess != null && !_voiceVoxProcess.HasExited)
                {
                    _voiceVoxProcess.Kill(true);
                    _voiceVoxProcess.Dispose();
                    _voiceVoxProcess = null;
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("VoiceVoxエンジンの終了に失敗しました: " + ex);
            }
        }
    }
}