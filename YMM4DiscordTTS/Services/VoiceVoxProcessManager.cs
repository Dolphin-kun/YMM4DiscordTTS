using System.Diagnostics;
using System.Windows;

namespace YMM4DiscordTTS.Services
{
    public class VoiceVoxProcessManager
    {
         public static VoiceVoxProcessManager Instance { get; } = new VoiceVoxProcessManager();

        // 外部からnewできないようにコンストラクタをprivateにする
        private VoiceVoxProcessManager() { }
        private Process? _voiceVoxProcess;

        public void StartEngineIfNotRunning()
        {
            var processes = Process.GetProcessesByName("run");
            if (processes.Length != 0)
            {
                Debug.WriteLine("VoiceVoxエンジンはすでに起動しています。");
                _voiceVoxProcess = processes[0];
                return;
            }

            try
            {
                // TODO: パスは設定ファイルなどから取得できるようにするとより良い
                string enginePath = @"user\resources\VOICEVOX\windows-directml\run.exe"; 
                var startInfo = new ProcessStartInfo
                {
                    FileName = enginePath,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                _voiceVoxProcess = Process.Start(startInfo);
                Debug.WriteLine("VoiceVoxエンジンを起動しました。");
            }
            catch (Exception ex)
            {
                MessageBox.Show("VoiceVoxエンジンの起動に失敗しました: " + ex.Message);
                Debug.WriteLine("VoiceVoxエンジン起動エラー: " + ex);
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
                    Debug.WriteLine("VoiceVoxエンジンを終了しました。");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("VoiceVoxエンジンの終了に失敗しました: " + ex);
            }
        }
    }
}