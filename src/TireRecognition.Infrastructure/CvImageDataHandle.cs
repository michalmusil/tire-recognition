using OpenCvSharp;
using TireRecognition.Domain;

namespace TireRecognition.Infrastructure;

public class CvImageDataHandle : ImageDataHandle, IDisposable
{
    public override ImageDimensions Dimensions => new(Width: Data.Cols, Height: Data.Rows);
    public override byte[] Bytes => GetInternalDataBytes();
    public Mat Data { get; }
    private byte[]? _bytes = null;

    public CvImageDataHandle(Mat data)
    {
        Data = data;
    }

    public void Dispose()
    {
        Data.Dispose();
    }

    private byte[] GetInternalDataBytes()
    {
        _bytes ??= Data.ToBytes();
        return _bytes;
    }
}