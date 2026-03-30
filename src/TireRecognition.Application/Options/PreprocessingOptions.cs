namespace TireRecognition.Application.Options;

public class PreprocessingOptions
{
    /// <summary>
    /// Defines max side (width or height) of image before preprocessing begins. Images exceeding this value are scaled down to match it. 
    /// </summary>
    public int MaxInputImageSize { get; set; }

    /// <summary>
    /// Defines max side (width or height) of image before recognition begins. Images exceeding this value are scaled down to match it. 
    /// </summary>
    public int MaxOutputImageSize { get; set; }

    /// <summary>
    /// How many slices should the extracted tire strip be sliced into (with overlap). Slicing serves to remove influence of width-heavy extracted tire sidewall aspect rations.  
    /// </summary>
    public int NumberOfSlices { get; set; }

    /// <summary>
    /// A ratio by which the detected rim radius is multiplied to get the whole wheel (incl. tire) radius 
    /// </summary>
    public double TireOuterRadiusRatio { get; set; }

    /// <summary>
    /// A ratio by the detected rim radius is multiplied to get the actually used rim radius (used for minor tuning of detection model results)  
    /// </summary>
    public double TireInnerRadiusRatio { get; set; }

    /// <summary>
    /// A width ratio by which the unwarped tire strip gets prolonged (by copying left side and appending it right) to prevent text splitting 
    /// </summary>
    public double TireStripProlongWidthRatio { get; set; }

    /// <summary>
    /// A width ratio by which the tire strip slices overlap - to prevent text splitting  
    /// </summary>
    public double SliceOverlapRatio { get; set; }

    /// <summary>
    /// Decimal confidence threshold for accepting results from tire rim segmentation model. 
    /// </summary>
    public double TireRimDetectionConfidenceThreshold { get; set; }

    /// <summary>
    /// A relative path (from bin root) to ONNX tire rim segmentation model 
    /// </summary>
    public string TireRimDetectionModelRelativePath { get; set; } = "";
}