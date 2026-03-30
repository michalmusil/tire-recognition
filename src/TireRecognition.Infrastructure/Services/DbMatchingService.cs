using System.Globalization;
using Fastenshtein;
using TireRecognition.Application.Repositories;
using TireRecognition.Application.Services;
using TireRecognition.Domain.DbMatching;
using TireRecognition.Domain.Postprocessing;

namespace TireRecognition.Infrastructure.Services;

public class DbMatchingService : IDbMatchingService
{
    private readonly ISupportedTireEntryRepository _tireEntryRepository;
    private readonly ISupportedManufacturerRepository _manufacturerRepository;

    public DbMatchingService(ISupportedTireEntryRepository tireEntryRepository,
        ISupportedManufacturerRepository manufacturerRepository)
    {
        _tireEntryRepository = tireEntryRepository;
        _manufacturerRepository = manufacturerRepository;
    }

    public async Task<List<TireDbMatch>> GetOrderedDbMatchesForTireCodeAsync(TireCode tireCode, int? limit)
    {
        var tireDbEntries = await _tireEntryRepository.GetSupportedTireEntries();
        var scoredEntryMatches = tireDbEntries
            .Select(entry => GetMatchForCodeAndDbEntry(tireCode, entry));

        var orderedMatches = scoredEntryMatches
            .OrderByDescending(m => m.MatchedMainParameterCount)
            .ThenBy(m => m.TotalRequiredCharEdits)
            .ToList();

        if (limit.HasValue)
            orderedMatches = orderedMatches
                .Take(limit.Value)
                .ToList();

        return orderedMatches;
    }

    public async Task<string?> GetManufacturerNameDbMatch(string rawTireManufacturerName)
    {
        var allSupportedManufacturers = await _manufacturerRepository.GetSupportedManufacturers();
        var matchingManufacturer = allSupportedManufacturers
            .FirstOrDefault(m =>
                m.Trim().Equals(rawTireManufacturerName.Trim(), StringComparison.OrdinalIgnoreCase));

        return matchingManufacturer;
    }

    private TireDbMatch GetMatchForCodeAndDbEntry(
        TireCode tireCode,
        TireDbEntry tireEntry
    )
    {
        var parameterMatches = new List<ParameterMatch>();
        var widthMatch = GetParameterMatch(tireCode.Width?.ToString() ?? "",
            tireEntry.Width.ToString(CultureInfo.InvariantCulture));
        parameterMatches.Add(widthMatch);

        var diameterMatch = GetParameterMatch(tireCode.Diameter?.ToString() ?? "",
            tireEntry.Diameter.ToString(CultureInfo.InvariantCulture));
        parameterMatches.Add(diameterMatch);

        var profileMatch = GetParameterMatch(tireCode.AspectRatio?.ToString() ?? "",
            tireEntry.Profile.ToString(CultureInfo.InvariantCulture));
        parameterMatches.Add(profileMatch);

        var constructionMatch = tireEntry.Construction is not null || tireCode.Construction is not null
            ? GetParameterMatch(tireCode.Construction ?? "",
                tireEntry.Construction ?? "")
            : null;
        if (constructionMatch is not null)
            parameterMatches.Add(constructionMatch);

        var loadIndexMatch = GetParameterMatch(tireCode.LoadIndex?.ToString() ?? "",
            tireEntry.LoadIndex?.ToString(CultureInfo.InvariantCulture) ?? "");
        parameterMatches.Add(loadIndexMatch);

        var loadIndex2Match = tireEntry.LoadIndex2 is not null || tireCode.LoadIndex2 is not null
            ? GetParameterMatch(tireCode.LoadIndex2?.ToString() ?? "",
                tireEntry.LoadIndex2?.ToString(CultureInfo.InvariantCulture) ?? "")
            : null;
        if (loadIndex2Match is not null)
            parameterMatches.Add(loadIndex2Match);

        var speedIndexMatch = GetParameterMatch(tireCode.SpeedRating ?? "",
            tireEntry.SpeedIndex ?? "");
        parameterMatches.Add(speedIndexMatch);

        var tireCodeString = tireCode.GetProcessedCode();
        var tireEntryString = tireEntry.GetTireCodeString();

        var totalDistance = parameterMatches.Sum(m => m.RequiredCharEdits);
        var estimatedAccuracy = GetAccuracyForLevenshteinDistance(totalDistance, tireCodeString, tireEntryString);
        var matchedMainParameterCount = parameterMatches
            .Take(4) // width, diameter, profile, construction 
            .Count(m => m.MatchesExactly);

        return new TireDbMatch(
            TireEntry: tireEntry,
            TotalRequiredCharEdits: totalDistance,
            EstimatedAccuracy: estimatedAccuracy,
            MatchedMainParameterCount: matchedMainParameterCount,
            WidthMatch: widthMatch,
            DiameterMatch: diameterMatch,
            ProfileMatch: profileMatch,
            ConstructionMatch: constructionMatch,
            LoadIndexMatch: loadIndexMatch,
            LoadIndex2Match: loadIndex2Match,
            SpeedIndexMatch: speedIndexMatch
        );
    }

    private ParameterMatch GetParameterMatch(string parameter1, string parameter2)
    {
        var levenshtein = new Levenshtein(parameter1);
        var distance = levenshtein.DistanceFrom(parameter2);
        var estimatedAccuracy = GetAccuracyForLevenshteinDistance(distance, parameter1, parameter2);

        return new ParameterMatch(distance, estimatedAccuracy);
    }

    private decimal GetAccuracyForLevenshteinDistance(int distance, string string1, string string2)
    {
        var stringLength = Math.Max((decimal)string1.Length, (decimal)string2.Length);
        if (stringLength == 0)
            return 0;
        return 1 - distance / stringLength;
    }
}