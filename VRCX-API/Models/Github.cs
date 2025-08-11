namespace VRCX_API.Models
{
    public class ReleaseAsset : GitHub.Models.ReleaseAsset
    {
        public string? Digest { get; set; }
    }

    public class Release : GitHub.Models.Release
    {
        public new List<ReleaseAsset>? Assets { get; set; }
    }
}
