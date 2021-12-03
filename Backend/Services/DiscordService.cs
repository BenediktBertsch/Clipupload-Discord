using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Options;

namespace Backend.Services
{
    public class DiscordService
    {
        private static DiscordSocketClient _client;
        private static DataService _dataService;
        private readonly DiscordSettings _discordSettings;
        public DiscordService(DataService dataService, IOptions<DiscordSettings> discordSettings)
        {
            // Start bot
            _dataService = dataService;
            _discordSettings = discordSettings.Value;
            _client = new DiscordSocketClient();
            _client.Ready += ReadyAsync;
            _client.LoginAsync(TokenType.Bot, _discordSettings.TokenBot).Wait();
            _client.StartAsync().Wait();
            Console.WriteLine("Started Bot...");
        }

        internal Task ReadyAsync()
        {
            Console.WriteLine($"{_client.CurrentUser} is connected!");
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

        internal static string GetUsernameOfId(ulong id)
        {
            return _client.GetUser(Convert.ToUInt64(id)).Username;
        }

        internal static async Task<ulong> GetId(string accessToken)
        {
            return await _dataService.GetUserId(accessToken);
        }
    }
}
