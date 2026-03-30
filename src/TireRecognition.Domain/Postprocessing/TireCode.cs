using System.Text;

namespace TireRecognition.Domain.Postprocessing;

public class TireCode
{
    public required string RawCode { get; set; }
    public decimal? Width { get; set; }
    public decimal? AspectRatio { get; set; }
    public string? DeprecatedSpeedRating { get; set; }
    public string? Construction { get; set; }
    public decimal? Diameter { get; set; }

    public char? LoadRange { get; set; }

    // The only load index for passenger cars, single-mount tire load index for Light Trucks
    public int? LoadIndex { get; set; }

    // Dual mount tire load index. Only present on light truck tires.
    public int? LoadIndex2 { get; set; }
    public string? SpeedRating { get; set; }

    public bool WasProcessedSuccessfully => Width is not null || AspectRatio is not null ||
                                            Construction is not null || Diameter is not null || LoadRange is not null ||
                                            LoadIndex is not null || LoadIndex2 is not null || SpeedRating is not null;

    public bool IsValidForReturn => Width is not null && AspectRatio is not null && Construction is not null &&
                                    Diameter is not null;

    public string GetProcessedCode()
    {
        var builder = new StringBuilder()
            .Append(Width);

        if (Width.HasValue || AspectRatio.HasValue)
            builder
                .Append('/');

        builder
            .Append(AspectRatio)
            .Append(DeprecatedSpeedRating)
            .Append(Construction)
            .Append(Diameter)
            .Append(LoadRange);

        if (LoadIndex is not null || LoadIndex2 is not null || SpeedRating is not null)
            builder.Append(' ');

        builder.Append(LoadIndex);
        if (LoadIndex2.HasValue)
            builder
                .Append('/')
                .Append(LoadIndex2);

        builder.Append(SpeedRating);
        return builder.ToString();
    }
}