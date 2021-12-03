namespace Backend
{
    public class GoogleSettings
    {
        public string ServiceMail { get; set; }
        public string ApiKey { get; set; }
    }
    public class DiscordSettings
    {
        public string EndPoint { get; set; }
        public string ScopeBot { get; set; }
        public string IdBot { get; set; }
        public string SecretBot { get; set; }
        public string TokenBot { get; set; }
        public string GuildId { get; set; }
        public string RoleId { get; set; }
        public string ChannelId { get; set; }
    }
    public class MigrationSettings
    {
        public string MigrationString { get; set; }
        public string ClipsPath { get; set; }
    }
    public class DatabaseSettings
    {
        public string StandardString { get; set; }
    }
    public class AppSettings
    {
        public string Backend { get; set; }
        public  string FrontEnd { get; set; }
    }
}
