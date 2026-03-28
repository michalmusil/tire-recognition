namespace TireRecognition.Domain;

public abstract class ImageDataHandle
{
    public abstract ImageDimensions Dimensions { get; }
    public abstract byte[] Bytes { get; }
}