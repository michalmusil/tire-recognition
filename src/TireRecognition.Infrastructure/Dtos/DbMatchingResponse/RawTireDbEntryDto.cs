using System.Text.RegularExpressions;
using TireRecognition.Domain.DbMatching;

namespace TireRecognition.Infrastructure.Dtos.DbMatchingResponse;

public record RawTireDbEntryDto(
    string ProductSizeWidth,
    string ProductSizeDiameter,
    string ProductSizeProfile,
    string? ProductConstruction,
    string? ProductLi,
    string? ProductLi2,
    string? ProductSi,
    string? ProductSi2,
    string? ProductLisi
)
{
    public TireDbEntry ToDomain()
    {
        var parsedWidth = decimal.TryParse(ProductSizeWidth, out var width);
        var parsedDiameter = decimal.TryParse(ProductSizeDiameter, out var diameter);
        var parsedProfile = decimal.TryParse(ProductSizeProfile, out var profile);
        var parsedLoadIndex = int.TryParse(ProductLi, out var li);

        int? loadIndex2 = null;
        string? speedIndex = null;

        if (ProductLisi is not null)
        {
            var lisiParts = ProductLisi.Split('/');
            if (lisiParts.Length == 2)
            {
                var match = Regex.Match(lisiParts[1], @"^\d{2,3}");
                var li2String = match.Success ? match.Value : null;
                var li2Parsed = int.TryParse(li2String, out var li2);
                loadIndex2 = li2Parsed ? li2 : null;
            }
        }

        if (ProductSi is not null)
        {
            var curedSpeedIndex = ProductSi
                .TrimStart('0')
                .Replace("_", string.Empty);
            speedIndex = curedSpeedIndex;
        }

        return new TireDbEntry
        {
            Width = parsedWidth ? width : 0,
            Diameter = parsedDiameter ? diameter : 0,
            Profile = parsedProfile ? profile : 0,
            Construction = ProductConstruction,
            LoadIndexSpeedIndex = ProductLisi,
            LoadIndex = parsedLoadIndex ? li : 0,
            LoadIndex2 = loadIndex2,
            SpeedIndex = speedIndex
        };
    }
}