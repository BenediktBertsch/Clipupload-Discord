namespace Backend
{
    public class DiscordSettings
    {
        public required string EndPoint { get; set; }
        public required string ScopeBot { get; set; }
        public required string IdBot { get; set; }
        public required string SecretBot { get; set; }
        public required string TokenBot { get; set; }
        public required string GuildId { get; set; }
        public required string RoleId { get; set; }
        public required string ChannelId { get; set; }
    }
    public class MigrationSettings
    {
        public required string MigrationString { get; set; }
        public required string ClipsPath { get; set; }
    }
    public class DatabaseSettings
    {
        public required string StandardString { get; set; }
    }
    public class AppSettings
    {
        public required string Backend { get; set; }
        public required string FrontEnd { get; set; }
    }
}
