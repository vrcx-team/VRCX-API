namespace VRCX_API.Configs
{
    public class CommonConfig
    {
        public static Config<CommonConfig> Config { get; } = new("CommonConfig.json");

        public string ErrorWebhookSnowFlake { get; set; } = string.Empty;
        public string ErrorWebhookToken { get; set; } = string.Empty;

        public string MongoDBAddress { get; set; } = string.Empty;
        public string CloudflareAPIKey { get; set; } = string.Empty;
        public string GithubAPIKey { get; set; } = string.Empty;
        public string SentryDsn { get; set; } = string.Empty;
    }
}
