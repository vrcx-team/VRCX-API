namespace VRCX_API.Configs
{
    public class VrcxConfig
    {
        public static Config<VrcxConfig> Config { get; } = new("VrcxConfig.json");

        public string SentryDsn { get; set; } = string.Empty;
    }
}
