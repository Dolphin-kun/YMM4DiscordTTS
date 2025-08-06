using System.Text.RegularExpressions;
using YMM4DiscordTTS.Settings;

namespace YMM4DiscordTTS.Helpers
{
    public static class TextHelper
    {
        private const string UrlPattern = @"https?://[^\s]+";
        private static int MaxLength => TTSSettings.Default.MaxTextLength;

        public static string ProcessForTTS(string originalText)
        {
            if (string.IsNullOrEmpty(originalText))
            {
                return string.Empty;
            }

            string processedText = originalText;

            // 辞書リストの各項目について、単語の置換を行う
            foreach (var entry in TTSSettings.Default.DictionaryEntries)
            {
                if (!string.IsNullOrEmpty(entry.Before))
                {
                    processedText = processedText.Replace(entry.Before, entry.After);
                }
            }

            // URLを置換
            processedText = Regex.Replace(processedText, UrlPattern, "URL省略");

            // 文字数制限
            if (processedText.Length >= MaxLength)
            {
                return string.Concat(processedText.AsSpan(0, MaxLength - 1), "、以下省略");
            }

            return processedText;
        }

        public static IEnumerable<string> SplitIntoSentences(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return [];
            }

            // 「。」「！」「？」または改行で文章を区切る正規表現
            var pattern = @"[^。！？\n]+(?:[。！？\n]|$)";
            return Regex.Matches(text, pattern)
                        .Cast<Match>()
                        .Select(m => m.Value.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s));
        }
    }
}
