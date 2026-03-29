namespace TireRecognition.Domain.Preprocessing;

public abstract class ImageDataHandle : IDisposable
{
    public abstract ImageDimensions Dimensions { get; }
    public abstract byte[] Bytes { get; }

    public abstract ImageDataHandle Clone();
    public abstract void Dispose();
}