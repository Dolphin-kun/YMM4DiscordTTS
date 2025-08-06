using Discord.Audio;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;
using YMM4DiscordTTS.Commands;
using YMM4DiscordTTS.Helpers;
using YMM4DiscordTTS.Settings;

namespace YMM4DiscordTTS.Services
{
    public class TTSRequest(ulong userId, string text)
    {
        public ulong UserId { get; } = userId;
        public string Text { get; } = text;
    }

    internal class TTSOrchestrator
    {
        private readonly ConcurrentQueue<TTSRequest> _ttsQueue = new();
        private readonly ConcurrentQueue<byte[]> _audioQueue = new();
        private readonly VoiceVoxHelper _voiceVox = new();

        private CancellationTokenSource? _mainCts;
        private CancellationTokenSource? _playbackInterruptCts;
        private volatile bool _isMessageSkipRequested = false;

        public void Start(AudioOutStream pcmStream)
        {
            Stop();
            _mainCts = new CancellationTokenSource();
            var token = _mainCts.Token;
            _ = Task.Run(() => TTSGenerationLoopAsync(token), token);
            _ = Task.Run(() => AudioPlaybackLoopAsync(pcmStream, token), token);
        }

        public void Stop()
        {
            if (_mainCts == null)
            {
                return;
            }

            if (!_mainCts.IsCancellationRequested)
            {
                _mainCts.Cancel();
            }

            _mainCts.Dispose();
            _mainCts = null;

            _ttsQueue.Clear();
            _audioQueue.Clear();
        }

        public void EnqueueText(ulong userId, string text)
        {
            _ttsQueue.Enqueue(new TTSRequest(userId, text));
        }

        public void Skip()
        {
            _playbackInterruptCts?.Cancel();
            _isMessageSkipRequested = true;
            _ttsQueue.Clear();
            _audioQueue.Clear();
            Debug.WriteLine("スキップが要求されました。");
        }

        public async Task<List<VoiceVoxSpeaker>> GetAvailableSpeakersAsync()
        {
            return await _voiceVox.GetSpeakersAsync();
        }

        private async Task TTSGenerationLoopAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_ttsQueue.TryDequeue(out var request))
                {
                    _isMessageSkipRequested = false;
                    float normalSpeed = TTSSettings.Default.NormalSpeed;
                    float fastSpeed = normalSpeed * 1.5f;
                    int longTextThreshold = TTSSettings.Default.LongTextThreshold;

                    string processedText = TextHelper.ProcessForTTS(request.Text);
                    float currentSpeed = request.Text.Length >= longTextThreshold ? fastSpeed : normalSpeed;
                    var sentences = TextHelper.SplitIntoSentences(processedText);

                    foreach (var sentence in sentences)
                    {
                        if (_isMessageSkipRequested || cancellationToken.IsCancellationRequested) break;
                        try
                        {
                            int speakerId = SpeakerCommand.GetSpeakerForUser(request.UserId);
                            var wavData = await _voiceVox.SynthesizeAsync(sentence, speakerId, currentSpeed);
                            _audioQueue.Enqueue(wavData);
                        }
                        catch (Exception ex) { Debug.WriteLine($"音声生成エラー: {ex}"); }
                    }
                }
                else
                {
                    await Task.Delay(100, cancellationToken);
                }
            }
        }

        private async Task AudioPlaybackLoopAsync(AudioOutStream pcmStream, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_audioQueue.TryDequeue(out var wavData))
                {
                    _playbackInterruptCts = new CancellationTokenSource();
                    try
                    {
                        await VoicePlayer.PlayVoice(pcmStream, wavData, _playbackInterruptCts.Token);
                    }
                    catch (OperationCanceledException) { Debug.WriteLine("再生が中断されました。"); }
                    catch (Exception ex) { Debug.WriteLine($"音声再生エラー: {ex}"); }
                    finally
                    {
                        _playbackInterruptCts.Dispose();
                        _playbackInterruptCts = null;
                    }
                }
                else
                {
                    await Task.Delay(100, cancellationToken);
                }
            }
        }
    }
}
