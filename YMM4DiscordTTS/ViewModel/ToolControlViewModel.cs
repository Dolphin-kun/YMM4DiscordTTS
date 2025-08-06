using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Discord;
using Discord.WebSocket;
using YMM4DiscordTTS.Services;
using YMM4DiscordTTS.Settings;

namespace YMM4DiscordTTS.ViewModel
{
    public class ToolControlViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static ToolControlViewModel Instance { get; } = new ToolControlViewModel();

        private readonly TTSOrchestrator _ttsOrchestrator;
        private SocketVoiceChannel? _currentVoiceChannel;

        #region Properties
        private string _token = "";
        public string Token
        {
            get => _token;
            set { if (_token != value) { _token = value; OnPropertyChanged(); } }
        }

        private bool _isPasswordVisible;
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set { if (_isPasswordVisible != value) { _isPasswordVisible = value; OnPropertyChanged(); } }
        }

        private bool _isLoggedIn;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set { if (_isLoggedIn != value) { _isLoggedIn = value; OnPropertyChanged(); } }
        }

        private bool _isConnectedToVoice;
        public bool IsConnectedToVoice
        {
            get => _isConnectedToVoice;
            set
            {
                if (_isConnectedToVoice != value)
                {
                    _isConnectedToVoice = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private ObservableCollection<SocketGuild> _guilds = [];
        public ObservableCollection<SocketGuild> Guilds
        {
            get => _guilds;
            set { if (_guilds != value) { _guilds = value; OnPropertyChanged(); } }
        }

        private SocketGuild? _selectedGuild;
        public SocketGuild? SelectedGuild
        {
            get => _selectedGuild;
            set
            {
                if (_selectedGuild != value)
                {
                    _selectedGuild = value;
                    OnPropertyChanged();
                    UpdateChannelLists();
                }
            }
        }

        private ObservableCollection<SocketTextChannel> _textChannels = [];
        public ObservableCollection<SocketTextChannel> TextChannels
        {
            get => _textChannels;
            set { if (_textChannels != value) { _textChannels = value; OnPropertyChanged(); } }
        }

        private SocketTextChannel? _selectedTextChannel;
        public SocketTextChannel? SelectedTextChannel
        {
            get => _selectedTextChannel;
            set { if (_selectedTextChannel != value) { _selectedTextChannel = value; OnPropertyChanged(); } }
        }

        private ObservableCollection<SocketVoiceChannel> _voiceChannels = [];
        public ObservableCollection<SocketVoiceChannel> VoiceChannels
        {
            get => _voiceChannels;
            set { if (_voiceChannels != value) { _voiceChannels = value; OnPropertyChanged(); } }
        }

        private SocketVoiceChannel? _selectedVoiceChannel;
        public SocketVoiceChannel? SelectedVoiceChannel
        {
            get => _selectedVoiceChannel;
            set { if (_selectedVoiceChannel != value) { _selectedVoiceChannel = value; OnPropertyChanged(); } }
        }

        private string _textToRead = "";
        public string TextToRead
        {
            get => _textToRead;
            set
            {
                if (_textToRead != value)
                {
                    _textToRead = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }
        #endregion

        public ICommand LoginCommand { get; }
        public ICommand JoinVoiceChannelCommand { get; }
        public ICommand LeaveVoiceChannelCommand { get; }
        public ICommand SkipPlaybackCommand { get; }
        public ICommand ReadAloudCommand { get; }

        private ToolControlViewModel()
        {
            _ttsOrchestrator = new TTSOrchestrator();

            LoginCommand = new RelayCommand(async _ => await ExecuteLoginAsync(), _ => !IsLoggedIn);
            JoinVoiceChannelCommand = new RelayCommand(async _ => await ExecuteJoinVoiceChannelAsync(), _ => CanExecuteJoinVoiceChannel());
            LeaveVoiceChannelCommand = new RelayCommand(async _ => await ExecuteLeaveVoiceChannelAsync(), _ => IsConnectedToVoice);
            SkipPlaybackCommand = new RelayCommand(_ => _ttsOrchestrator.Skip());
            ReadAloudCommand = new RelayCommand(_ => ExecuteReadAloud(), _ => CanExecuteReadAloud());

            DiscordService.Instance.Log += OnLogReceived;
            DiscordService.Instance.Ready += OnDiscordReady;
            DiscordService.Instance.MessageReceived += OnDiscordMessageReceived;
            DiscordService.Instance.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
            DiscordService.Instance.DisconnectedFromVoice += OnDisconnectedFromVoice;

            Token = TTSSettings.Default.Token ?? "";
            _ = InitializeAsync();
        }

        public void SkipPlayback() => _ttsOrchestrator.Skip();

        private async Task InitializeAsync()
        {
            if (TTSSettings.Default.IsCheckVersion && await GetVersion.CheckVersionAsync("YMM4Discord読み上げ"))
            {
                string url =
                    "https://ymm4-info.net/ymme/YMM4Discord%E8%AA%AD%E3%81%BF%E4%B8%8A%E3%81%92%E3%83%97%E3%83%A9%E3%82%B0%E3%82%A4%E3%83%B3";
                var result = MessageBox.Show(
                    $"新しいバージョンがあります。\n\n最新バージョンを確認しますか？\nOKを押すと配布サイトが開きます。\n{url}",
                    "YMM4Discord読み上げプラグイン",
                    MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
            }
            VoiceVoxProcessManager.Instance.StartEngineIfNotRunning();

            if (!string.IsNullOrWhiteSpace(Token))
            {
                await ConnectToDiscordAsync(Token);
            }

            await LoadSpeakersAsync();

        }

        private async Task ExecuteLoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Token))
            {
                MessageBox.Show("トークンが空です。");
                return;
            }
            await ConnectToDiscordAsync(Token);
        }

        private bool CanExecuteJoinVoiceChannel() => SelectedGuild != null && SelectedVoiceChannel != null && !IsConnectedToVoice;

        private async Task ExecuteJoinVoiceChannelAsync()
        {
            if (SelectedVoiceChannel is not SocketVoiceChannel vc) return;

            try
            {
                await DiscordService.Instance.JoinVoiceChannelAsync(vc);
                _currentVoiceChannel = vc;

                var pcmStream = DiscordService.Instance.CreatePCMStream();
                if (pcmStream != null)
                {
                    _ttsOrchestrator.Start(pcmStream);
                    _ttsOrchestrator.EnqueueText(0, $"{vc.Name}に接続しました");
                    IsConnectedToVoice = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"VC接続失敗: {ex.Message}");
                IsConnectedToVoice = false;
            }
        }

        private static async Task ExecuteLeaveVoiceChannelAsync()
        {
            await DiscordService.Instance.LeaveVoiceChannelAsync();
        }

        private bool CanExecuteReadAloud()
        {
            return IsConnectedToVoice && !string.IsNullOrWhiteSpace(TextToRead);
        }

        private void ExecuteReadAloud()
        {
            if (!CanExecuteReadAloud()) return;

            try
            {
                _ttsOrchestrator.EnqueueText(0, TextToRead);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"読み上げの追加に失敗しました: {ex.Message}");
            }
        }

        private async Task ConnectToDiscordAsync(string token)
        {
            try
            {
                await DiscordService.Instance.LoginAndStartAsync(token);
                IsLoggedIn = true;
            }
            catch (Exception ex)
            {
                IsLoggedIn = false;
                MessageBox.Show($"Discordへのログインに失敗しました: {ex.Message}");
            }
        }

        private void UpdateChannelLists()
        {
            if (SelectedGuild is not SocketGuild guild)
            {
                TextChannels.Clear();
                VoiceChannels.Clear();
                return;
            }

            var botUser = guild.CurrentUser;
            var sortedTextChannels = guild.TextChannels
                .Where(c =>
                {
                    if (c.ChannelType is not (ChannelType.Text or ChannelType.Voice)) return false;
                    var perms = botUser.GetPermissions(c);
                    return perms.Has(ChannelPermission.ViewChannel) && perms.Has(ChannelPermission.SendMessages);
                })
                .OrderBy(c => c.Category?.Position ?? -1)
                .ThenBy(c => c.Position)
                .ToList();
            TextChannels = new ObservableCollection<SocketTextChannel>(sortedTextChannels);

            var sortedVoiceChannels = guild.VoiceChannels
                .Where(c =>
                {
                    var perms = botUser.GetPermissions(c);
                    return perms.Has(ChannelPermission.Connect) && perms.Has(ChannelPermission.Speak);
                })
                .OrderBy(c => c.Category?.Position ?? -1)
                .ThenBy(c => c.Position)
                .ToList();
            VoiceChannels = new ObservableCollection<SocketVoiceChannel>(sortedVoiceChannels);
        }

        private async Task OnLogReceived(LogMessage msg)
        {
            if (msg.Exception is Discord.Net.HttpException httpEx &&
                httpEx.HttpCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await Application.Current.Dispatcher.Invoke(async () =>
                {
                    MessageBox.Show("Discordへのログインに失敗しました。\nトークンが無効です。設定を確認してください。");

                    Token = string.Empty;
                    IsLoggedIn = false;

                    await DiscordService.Instance.LogoutAndStopAsync();
                });
            }

        }

        private Task OnDiscordReady()
        {
            return Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Guilds = new ObservableCollection<SocketGuild>(DiscordService.Instance.Guilds);
            }).Task;
        }

        private Task OnDiscordMessageReceived(SocketMessage message)
        {
            if (message is not SocketUserMessage userMessage || userMessage.Author.IsBot)
                return Task.CompletedTask;

            if (SelectedTextChannel != null && userMessage.Channel.Id == SelectedTextChannel.Id)
            {
                _ttsOrchestrator.EnqueueText(userMessage.Author.Id, userMessage.Content);
            }
            return Task.CompletedTask;
        }

        private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            if (user.Id == DiscordService.Instance.CurrentUser?.Id)
            {
                return Task.CompletedTask;
            }

            var userName = (user as SocketGuildUser)?.DisplayName ?? user.GlobalName ?? user.Username;

            if (TTSSettings.Default.IsJoinAnnouncement && before.VoiceChannel is null && after.VoiceChannel is not null)
            {
                if (_currentVoiceChannel != null && after.VoiceChannel.Id == _currentVoiceChannel.Id)
                {
                    _ttsOrchestrator.EnqueueText(0, $"{userName}さんが入室しました");
                }
            }

            if (TTSSettings.Default.IsLeaveAnnouncement && before.VoiceChannel is not null && after.VoiceChannel is null)
            {
                if (_currentVoiceChannel != null && before.VoiceChannel.Id == _currentVoiceChannel.Id)
                {
                    _ttsOrchestrator.EnqueueText(0, $"{userName}さんが退出しました");
                }
            }

            return Task.CompletedTask;
        }

        private Task OnDisconnectedFromVoice()
        {
            _ttsOrchestrator.Stop();
            _currentVoiceChannel = null;
            IsConnectedToVoice = false;
            return Task.CompletedTask;
        }

        private async Task LoadSpeakersAsync()
        {
            try
            {
                var speakers = await _ttsOrchestrator.GetAvailableSpeakersAsync();
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var settings = TTSSettings.Default;
                    if (speakers != null && speakers.Count != 0)
                    {
                        var currentId = settings.SpeakerId;

                        settings.AvailableSpeakers.Clear();
                        foreach (var speaker in speakers)
                        {
                            settings.AvailableSpeakers.Add(speaker);
                        }

                        if (settings.AvailableSpeakers.Any(s => s.Id == currentId))
                        {
                            settings.SpeakerId = currentId;
                        }
                        else if (settings.AvailableSpeakers.Any())
                        {
                            settings.SpeakerId = settings.AvailableSpeakers.First().Id;
                        }
                    }
                });
            }
            catch (Exception)
            {
                MessageBox.Show("VOICEVOXエンジンから話者リストを取得できませんでした。VOICEVOXを使用したことがない方は音声の生成を一度行う必要があります。");
            }
        }
    }
}