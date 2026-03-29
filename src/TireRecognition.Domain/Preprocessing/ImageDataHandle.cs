namespace TireRecognition.Domain.Preprocessing;

public abstract class ImageDataHandle
{
    public abstract ImageDimensions Dimensions { get; }
    public abstract byte[] Bytes { get; }
}