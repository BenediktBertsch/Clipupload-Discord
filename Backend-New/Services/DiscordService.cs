using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Backend.Services
{
    public sealed class DiscordService
    {
        private static DiscordSocketClient _client = null!;
        private static DataService _dataService = null!;
        private readonly DiscordSettings _discordSettings;
        public DiscordService(DataService dataService, IOptions<DiscordSettings> discordSettings)
        {
            // Start bot
            _dataService = dataService;
            _ = _dataService ?? throw new ArgumentNullException("Variable is null! Unexpected Behaviour!", nameof(_dataService));
            _discordSettings = discordSettings.Value;
            _client = new DiscordSocketClient();
            _client.Ready += ReadyAsync;
            _client.LoginAsync(TokenType.Bot, _discordSettings.TokenBot).Wait();
            _client.StartAsync().Wait();
            Debug.WriteLine("Started Bot");
        }

        internal Task ReadyAsync()
        {
            Debug.WriteLine($"{_client.CurrentUser} is connected!");
            return Task.CompletedTask;
        }

        internal async Task PostVideo(string url)
        {
            var channel = _client.GetGuild(Convert.ToUInt64(_discordSettings.GuildId)).GetTextChannel(Convert.ToUInt64(_discordSettings.ChannelId));
            await channel.SendMessageAsync(url);
            
        }

        internal bool CheckUserGroup(ulong userId)
        {
            var user = _client.GetGuild(Convert.ToUInt64(_discordSettings.GuildId)).GetUser(userId);
            if(user == null)
            {
                return false;
            }
            return user.Roles.Any(e => e.Id == Convert.ToUInt64(_discordSettings.RoleId));
        }

        internal static async Task<string> GetUsernameOfId(ulong id)
        {
            var user = await _client.GetUserAsync(id);
            return user.Username;
        }

        internal static async Task<ulong> GetId(string accessToken)
        {
            return await _dataService.GetUserId(accessToken);
        }
    }
}
