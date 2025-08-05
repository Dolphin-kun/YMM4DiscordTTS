using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace YMM4DiscordTTS.Services
{
    public class VoiceVoxSpeaker
    {
        public string Name { get; set; } = "";
        public int Id { get; set; }
    }

    public class VoiceVoxHelper 
    {
        private readonly HttpClient _client = new()
        {
            BaseAddress = new Uri("http://127.0.0.1:50021/")
        };

        public async Task<byte[]> SynthesizeAsync(string text, int speakerId = 1, float speed = 1.0f)
        {
            var queryResponse = await _client.PostAsync($"audio_query?text={Uri.EscapeDataString(text)}&speaker={speakerId}", null);
            queryResponse.EnsureSuccessStatusCode();
            var queryJsonNode = await queryResponse.Content.ReadFromJsonAsync<JsonNode>();

            if (queryJsonNode != null)
            {
                queryJsonNode["speedScale"] = speed;
            }

            var synthesisResponse = await _client.PostAsJsonAsync($"synthesis?speaker={speakerId}", queryJsonNode);
            synthesisResponse.EnsureSuccessStatusCode();

            return await synthesisResponse.Content.ReadAsByteArrayAsync();
        }

        public async Task<List<VoiceVoxSpeaker>> GetSpeakersAsync()
        {
            var speakersList = new List<VoiceVoxSpeaker>();
            var response = await _client.GetAsync("speakers");
            response.EnsureSuccessStatusCode();

            var jsonNodes = await response.Content.ReadFromJsonAsync<JsonNode[]>();
            if (jsonNodes is null) return speakersList;

            foreach (var node in jsonNodes)
            {
                if (node is null) continue;
                string characterName = node["name"]?.ToString() ?? "不明";
                var styles = node["styles"]?.AsArray();
                if (styles is null) continue;

                foreach (var style in styles)
                {
                    if (style is null) continue;
                    speakersList.Add(new VoiceVoxSpeaker
                    {
                        Name = $"{characterName} ({(style["name"]?.ToString() ?? "ノーマル")})",
                        Id = style["id"]?.GetValue<int>() ?? 0
                    });
                }
            }
            return [.. speakersList.OrderBy(s => s.Id)];
        }
    }
}
