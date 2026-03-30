using System.Text.RegularExpressions;
using TireRecognition.Application.Services;
using TireRecognition.Domain.Postprocessing;

namespace TireRecognition.Infrastructure.Services;

public class PostprocessingService : IPostprocessingService
{
    public async Task<IEnumerable<TireCode>> ExtractStructuredTireCodesAsync(string rawTireCode)
    {
        var cleanedUpTireCode = PerformPreMatchingCleanup(rawTireCode);
        var anchors = GetTireCodeAnchorsFromRawString(cleanedUpTireCode).ToList();

        if (!anchors.Any())
            return [];

        var validTireCodes = anchors
            .Select(a => ProcessPotentialTireCode(a, cleanedUpTireCode))
            .Where(tc => tc.IsValidForReturn)
            .ToList();

        return validTireCodes;
    }

    public TireCode? PickBestTireCode(IEnumerable<TireCode> tireCodes)
    {
        var tireCodeList = tireCodes.ToList();
        if (!tireCodeList.Any())
            return null;

        var bestTireCode = tireCodeList
            .Select(tc => (tc, GetScoreOfTireCode(tc)))
            .OrderByDescending(x => x.Item2)
            .ThenByDescending(x => x.tc.GetProcessedCode().Length)
            .FirstOrDefault().tc;

        return bestTireCode;
    }

    private int GetScoreOfTireCode(TireCode tireCode)
    {
        var bonusMultiplierValue = 5;
        var score = 0;

        IncrementIfNotNull(tireCode.Width, ref score, bonusMultiplierValue);
        IncrementIfNotNull(tireCode.AspectRatio, ref score, bonusMultiplierValue);
        IncrementIfNotNull(tireCode.Construction, ref score, bonusMultiplierValue);
        IncrementIfNotNull(tireCode.Diameter, ref score, bonusMultiplierValue);
        IncrementIfNotNull(tireCode.LoadIndex, ref score);
        IncrementIfNotNull(tireCode.SpeedRating, ref score);

        return score;
    }

    private void IncrementIfNotNull(dynamic? value, ref int score, int multiplier = 1)
    {
        if (value is not null)
            score += 1 * multiplier;
    }

    private string PerformPreMatchingCleanup(string rawCode)
    {
        string cleanedUp = Regex.Replace(rawCode, @"[\n]", "\\");
        cleanedUp = cleanedUp
            .Replace("|", "/")
            .Replace(";", "")
            .Replace(":", "")
            .Replace(",", "");
        cleanedUp = Regex.Replace(cleanedUp, @"\s+", "|");
        cleanedUp = cleanedUp.ToUpperInvariant();

        return cleanedUp;
    }

    private IEnumerable<Match> GetTireCodeAnchorsFromRawString(string rawTireCode)
    {
        var regex = new Regex(@"/(?<AspectRatio>\d{2,3}|\d{1,2}\.\d{1,2})");
        return regex.Matches(rawTireCode);
    }

    private TireCode ProcessPotentialTireCode(Match tireCodeAnchorMatch, string code)
    {
        var tireCode = new TireCode
        {
            RawCode = code,
        };

        var aspectRatioValid = decimal.TryParse(tireCodeAnchorMatch.Groups["AspectRatio"].Value, out var aspectRatio);
        if (!aspectRatioValid)
            return tireCode;

        tireCode.AspectRatio = aspectRatio;

        var leftOfAnchor = tireCode.RawCode.Substring(0, tireCodeAnchorMatch.Index);
        ExtractSectionWidth(tireCode, leftOfAnchor);

        var rightOfAnchor = tireCode.RawCode.Substring(tireCodeAnchorMatch.Index + tireCodeAnchorMatch.Length);
        var constructionAndDeprecatedSpeedRatingCharCount = ExtractConstructionAndDeprecatedSpeedRating(
            tireCode,
            rightOfAnchor
        );
        rightOfAnchor = ExtractCharactersFromTail(rightOfAnchor, constructionAndDeprecatedSpeedRatingCharCount);

        var diameterCharCount = ExtractDiameter(tireCode, rightOfAnchor);
        rightOfAnchor = ExtractCharactersFromTail(rightOfAnchor, diameterCharCount);

        var rangeAndIndexCharCount = ExtractLoadRangeAndIndex(tireCode, rightOfAnchor);
        rightOfAnchor = ExtractCharactersFromTail(rightOfAnchor, rangeAndIndexCharCount);

        ExtractSpeedRating(tireCode, rightOfAnchor);

        return tireCode;
    }

    private int ExtractSectionWidth(TireCode code, string leftOfAspectRatio)
    {
        Match widthMatch = Regex.Match(leftOfAspectRatio, @"(?<Width>\d{3}|\d{1,2}\.\d{1,2})\|?$");
        var width = widthMatch.Success ? widthMatch.Groups["Width"].Value : null;
        if (width != null)
        {
            var widthIsValid = decimal.TryParse(width, out var widthValue);
            if (widthIsValid)
                code.Width = widthValue;
        }

        return widthMatch.Length;
    }

    private int ExtractConstructionAndDeprecatedSpeedRating(TireCode code, string rightOfAspectRatio)
    {
        Match constructionMatch = Regex.Match(rightOfAspectRatio,
            @"^\|?(?<DeprecatedSpeedRating>[A,B,C,D,E,F,G,J,K,L,M,N,P,Q,R,S,T,U,H,V,Z,W,Y]{1})?(?<Construction>[RDB]{1})\|?");
        if (constructionMatch.Success)
        {
            code.Construction = constructionMatch.Groups["Construction"].Success
                ? constructionMatch.Groups["Construction"].Value
                : null;
            code.DeprecatedSpeedRating = constructionMatch.Groups["DeprecatedSpeedRating"].Success
                ? constructionMatch.Groups["DeprecatedSpeedRating"].Value
                : null;
        }

        return constructionMatch.Length;
    }

    private int ExtractDiameter(TireCode code, string leftOfConstruction)
    {
        Match diameterMatch = Regex.Match(leftOfConstruction, @"^(?<Diameter>\d{1,3}|\d{1,2}\.\d{1,2})\|?");
        var diameter = diameterMatch.Success ? diameterMatch.Groups["Diameter"].Value : null;
        if (diameter is not null)
        {
            var diameterIsValid = int.TryParse(diameter, out var diameterValue);
            if (diameterIsValid)
                code.Diameter = diameterValue;
        }

        return diameterMatch.Length;
    }

    private int ExtractLoadRangeAndIndex(TireCode code, string leftOfDiameter)
    {
        char[] validLoadRanges = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'L', 'M', 'N'];

        Match loadRangeIndexMatch =
            Regex.Match(leftOfDiameter, @"^\|?(?<LoadRange>[A-Z]{1})?\|?(?<LoadIndex>(\d{1,3}/?)?\d{2,3})\|?");
        if (!loadRangeIndexMatch.Success)
            return 0;

        var loadIndex = loadRangeIndexMatch.Groups["LoadIndex"].Success
            ? loadRangeIndexMatch.Groups["LoadIndex"].Value
            : null;
        char? loadRange = loadRangeIndexMatch.Groups["LoadRange"].Success
            ? loadRangeIndexMatch.Groups["LoadRange"].Value.FirstOrDefault()
            : null;

        if (loadRange is not null && validLoadRanges.Contains(loadRange.Value))
            code.LoadRange = loadRange.Value;

        if (loadIndex is not null)
        {
            var loadIndices = loadIndex.Split('/');
            var firstLoadIndexValid = int.TryParse(loadIndices[0], out var li1);
            if (firstLoadIndexValid)
                code.LoadIndex = li1;

            if (loadIndices.Length > 1 && int.TryParse(loadIndices[1], out var li2))
                code.LoadIndex2 = li2;
        }

        return loadRangeIndexMatch.Length;
    }

    private int ExtractSpeedRating(TireCode code, string leftOfLoadIndex)
    {
        Match speedRatingMatch = Regex.Match(leftOfLoadIndex,
            @"^(?<SpeedRating>[A,B,C,D,E,F,G,J,K,L,M,N,P,Q,R,S,T,U,H,V,Z,W,Y]{1})\|?");
        var speedRating = speedRatingMatch.Success ? speedRatingMatch.Groups["SpeedRating"].Value : null;
        code.SpeedRating = speedRating;

        return speedRatingMatch.Length;
    }

    private string ExtractCharactersFromTail(string original, int numberOfChars)
    {
        if (numberOfChars < 1 || numberOfChars > original.Length)
            return original;

        return original.Substring(numberOfChars, original.Length - numberOfChars);
    }
}