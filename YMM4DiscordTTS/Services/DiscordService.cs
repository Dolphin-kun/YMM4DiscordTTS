using Discord;
using Discord.Audio;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using System.Windows;

namespace YMM4DiscordTTS.Services
{
    internal class DiscordService
    {
        public static DiscordService Instance { get; } = new DiscordService();

        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;
        private IAudioClient? _audioClient;
        private bool _areModulesLoaded = false;

        public event Func<SocketMessage, Task>? MessageReceived;
        public event Func<Task>? Ready;
        public event Func<SocketUser, SocketVoiceState, SocketVoiceState, Task>? UserVoiceStateUpdated;
        public event Func<Task>? DisconnectedFromVoice;

        public SocketSelfUser CurrentUser => _client.CurrentUser;
        public IReadOnlyCollection<SocketGuild> Guilds => _client.Guilds;

        private DiscordService()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent | GatewayIntents.GuildVoiceStates
            });
            _interactionService = new InteractionService(_client.Rest);

            _client.Log += msg => { System.Diagnostics.Debug.WriteLine(msg); return Task.CompletedTask; };
            _client.Ready += OnClientReady;
            _client.MessageReceived += OnMessageReceived;
            _client.InteractionCreated += OnInteractionCreated;
            _client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
        }

        public async Task LoginAndStartAsync(string token)
        {
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
        }

        public async Task JoinVoiceChannelAsync(IVoiceChannel channel)
        {
            if (_audioClient != null)
            {
                await LeaveVoiceChannelAsync();
            }

            _audioClient = await channel.ConnectAsync(selfDeaf: false);
            _audioClient.Disconnected += OnAudioClientDisconnected;
        }

        public async Task LeaveVoiceChannelAsync()
        {
            if (_audioClient != null)
            {
                await _audioClient.StopAsync();
            }
        }

        public AudioOutStream? CreatePCMStream()
        {
            return _audioClient?.CreatePCMStream(AudioApplication.Mixed);
        }

        private Task OnAudioClientDisconnected(Exception ex)
        {
            var clientToCleanup = Interlocked.Exchange(ref _audioClient, null);

            if (clientToCleanup != null)
            {
                clientToCleanup.Disconnected -= OnAudioClientDisconnected;
                clientToCleanup.Dispose();

                DisconnectedFromVoice?.Invoke();
            }
            return Task.CompletedTask;
        }

        public static async Task<IAudioClient> JoinAudioAsync(IVoiceChannel channel)
        {
            return await channel.ConnectAsync(selfDeaf: false);
        }

        private async Task OnClientReady()
        {
            if (!_areModulesLoaded)
            {
                await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), null);
                _areModulesLoaded = true;
            }

            foreach (var guild in _client.Guilds)
            {
                try
                {
                    var existingCommands = await guild.GetApplicationCommandsAsync();
                    var commandsToRegister = _interactionService.SlashCommands;

                    if (existingCommands.Count != commandsToRegister.Count)
                    {
                        System.Diagnostics.Debug.WriteLine($"{guild.Name} ({guild.Id}) にコマンドを登録します。");
                        await _interactionService.RegisterCommandsToGuildAsync(guild.Id);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"{guild.Name} ({guild.Id}) のコマンドは最新です。");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"{guild.Name} へのコマンド登録に失敗しました: {ex}");
                }
            }
            Ready?.Invoke();
        }

        private Task OnMessageReceived(SocketMessage message)
        {
            MessageReceived?.Invoke(message);
            return Task.CompletedTask;
        }

        private async Task OnInteractionCreated(SocketInteraction interaction)
        {
            var ctx = new SocketInteractionContext(_client, interaction);
            await _interactionService.ExecuteCommandAsync(ctx, null);
        }

        private Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState before, SocketVoiceState after)
        {
            UserVoiceStateUpdated?.Invoke(user, before, after);
            return Task.CompletedTask;
        }
    }
}
