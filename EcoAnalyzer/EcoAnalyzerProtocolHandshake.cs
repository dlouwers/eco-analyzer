using System.Linq;
using System.Text.RegularExpressions;

namespace EcoAnalyzer
{
    static class EcoAnalyzerProtocolHandshake
    {
        public static int Handshake(string message, int[] supportedVersions)
        {
            var clientVersions = from Capture version in Regex.Match(message, @"Supported-Versions: (\d+)(,\s*\d+)*").Captures
                                 select int.Parse(version.Value);
            return (from int version in clientVersions
                                where supportedVersions.Contains(version)
                                orderby version descending
                                select version).FirstOrDefault();
        }

        public static string UseVersionMessage(int version)
        {
            return $"Using-Version: {version}\n";
        }
    }
}
