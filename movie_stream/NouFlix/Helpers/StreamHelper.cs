using System.Text.RegularExpressions;

namespace NouFlix.Helpers;

public static class StreamHelper
{
    public static int EstimatePositionSeconds(string segmentFileName, int segmentDurationSeconds = 4)
    {
        var match = Regex.Match(segmentFileName, @"(\d+)");
        if (!match.Success)
            return 0;

        if (!int.TryParse(match.Groups[1].Value, out var index))
            return 0;
        
        return index * segmentDurationSeconds;
    }
}