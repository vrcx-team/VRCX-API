using System.Text.RegularExpressions;

namespace VRCX_API.Helpers
{
    public partial class LinkHeader
    {
        public string? FirstLink { get; set; }
        public string? PrevLink { get; set; }
        public string? NextLink { get; set; }
        public string? LastLink { get; set; }

        public static LinkHeader? FromHeader(string linkHeader)
        {
            LinkHeader? parsedLinkHeader = null;

            if (!string.IsNullOrWhiteSpace(linkHeader))
            {
                string[] linkStrings = linkHeader.Split(',');

                if (linkStrings != null && linkStrings.Length != 0)
                {
                    parsedLinkHeader = new();

                    foreach (string linkString in linkStrings)
                    {
                        var relMatch = RelRegex().Match(linkString);
                        var linkMatch = LinkRegex().Match(linkString);

                        if (relMatch.Success && linkMatch.Success)
                        {
                            string rel = relMatch.Value;
                            string link = linkMatch.Value;

                            if (rel.Equals("first", StringComparison.OrdinalIgnoreCase))
                            {
                                parsedLinkHeader.FirstLink = link;
                            }
                            else if (rel.Equals("prev", StringComparison.OrdinalIgnoreCase))
                            {
                                parsedLinkHeader.PrevLink = link;
                            }
                            else if (rel.Equals("next", StringComparison.OrdinalIgnoreCase))
                            {
                                parsedLinkHeader.NextLink = link;
                            }
                            else if (rel.Equals("last", StringComparison.OrdinalIgnoreCase))
                            {
                                parsedLinkHeader.LastLink = link;
                            }
                        }
                    }

                    return parsedLinkHeader;
                }
            }

            return parsedLinkHeader;
        }

        [GeneratedRegex("(?<=rel=\").+?(?=\")", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
        private static partial Regex RelRegex();

        [GeneratedRegex("(?<=<).+?(?=>)", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
        private static partial Regex LinkRegex();
    }
}
