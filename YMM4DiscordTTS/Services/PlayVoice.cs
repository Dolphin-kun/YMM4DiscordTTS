using Discord.Audio;
using NAudio.Wave;
using System.Diagnostics;
using System.IO;

namespace YMM4DiscordTTS.Services
{
    public static class VoicePlayer
    {
        public static async Task PlayVoice(AudioOutStream stream, byte[] wavData, CancellationToken cancellationToken)
        {
            using var ms = new MemoryStream(wavData);
            using var wavReader = new WaveFileReader(ms);
            using var resampler = new MediaFoundationResampler(wavReader, new WaveFormat(48000, 16, 2));
            resampler.ResamplerQuality = 60;

            var buffer = new byte[4096];

            try
            {
                int bytesRead;
                while ((bytesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await stream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                }
            }
            finally
            {
                await stream.FlushAsync(cancellationToken);
            }
        }
    }
}
