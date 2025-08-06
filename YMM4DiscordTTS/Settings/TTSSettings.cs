using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using YMM4DiscordTTS.Models;
using YMM4DiscordTTS.Services;
using YukkuriMovieMaker.Plugin;

namespace YMM4DiscordTTS.Settings
{
    internal class TTSSettings : SettingsBase<TTSSettings>
    {
        public override SettingsCategory Category => SettingsCategory.None;
        public override string Name => "YMM4DisocrdTTS";

        public override bool HasSettingView => true;
        public override object? SettingView => new PluginSettings();

        public string Token { get => token; set => Set(ref token, value); }
        private string token = "";

        [JsonIgnore]
        public ObservableCollection<VoiceVoxSpeaker> AvailableSpeakers { get; } = [];

        public ConcurrentDictionary<ulong, int> UserVoiceMappings { get => userVoiceMappings; set => Set(ref userVoiceMappings, value); }
        private ConcurrentDictionary<ulong, int> userVoiceMappings = new();

        //バージョンチェック
        public bool IsCheckVersion { get => isCheckVersion; set => Set(ref isCheckVersion, value); }
        private bool isCheckVersion = true;

        //読み上げ設定
        public float NormalSpeed { get => normalSpeed; set => Set(ref normalSpeed, value); }
        private float normalSpeed = 1.2f;

        public int LongTextThreshold { get => longTextThreshold; set => Set(ref longTextThreshold, value); }
        private int longTextThreshold = 100;

        public int MaxTextLength { get => maxTextLength; set => Set(ref maxTextLength, value); }
        private int maxTextLength = 200;

        public int SpeakerId { get => speakerId; set => Set(ref speakerId, value); }
        private int speakerId = 1;

        public bool IsJoinAnnouncement { get => isJoinAnnouncement; set => Set(ref isJoinAnnouncement, value); }
        private bool isJoinAnnouncement = true;
        public bool IsLeaveAnnouncement { get => isLeaveAnnouncement; set => Set(ref isLeaveAnnouncement, value); }
        private bool isLeaveAnnouncement = true;

        //辞書設定
        public ObservableCollection<DictionaryEntry> DictionaryEntries { get => _dictionaryEntries; set => Set(ref _dictionaryEntries, value); }
        private ObservableCollection<DictionaryEntry> _dictionaryEntries = [];


        public override void Initialize()
        {
            if (DictionaryEntries.Count == 0)
            {
                DictionaryEntries.Add(new DictionaryEntry { Before = "w", After = "わら" });
            }
        }
    }
}
