using Discord;
using Microsoft.Extensions.Options;
using Discord.Rest;

namespace Backend.Services
{
    public sealed class DiscordService
    {
        private static DiscordRestClient _restClient = null!;
        private static DataService _dataService = null!;
        private readonly DiscordSettings _discordSettings;
        public DiscordService(DataService dataService, IOptions<DiscordSettings> discordSettings)
        {
            _dataService = dataService;
            _ = _dataService ?? throw new ArgumentNullException("Variable is null! Unexpected Behaviour!", nameof(_dataService));
            _discordSettings = discordSettings.Value;
            _restClient = new DiscordRestClient();
            _restClient.LoginAsync(TokenType.Bot, _discordSettings.TokenBot);
        }

        internal async Task PostVideo(string url)
        {
            var channel = await (await _restClient.GetGuildAsync(Convert.ToUInt64(_discordSettings.GuildId))).GetTextChannelAsync(Convert.ToUInt64(_discordSettings.ChannelId));
            await channel.SendMessageAsync(url);
        }

        internal async Task<bool> CheckUserInGuild(ulong userId)
        {
            var user = await (await _restClient.GetGuildAsync(Convert.ToUInt64(_discordSettings.GuildId))).GetUserAsync(userId);
            if(user == null)
            {
                return false;
            }
            return user.RoleIds.Any(e => e == Convert.ToUInt64(_discordSettings.RoleId));
        }

        internal static async Task<string> GetUsernameById(ulong id)
        {
            var user = await _restClient.GetUserAsync(id);
            return user.Username;
        }
    }
}
