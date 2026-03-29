namespace TireRecognition.Infrastructure.Options;

public class PreprocessingOptions
{
    public int MaxInputImageSize { get; set; }
    public int MaxOutputImageSize { get; set; }
    public double TireOuterRadiusRatio { get; set; }
    public double TireInnerRadiusRatio { get; set; }
    public double TireStripProlongWidthRatio { get; set; }
    public double SliceOverlapRatio { get; set; }
}