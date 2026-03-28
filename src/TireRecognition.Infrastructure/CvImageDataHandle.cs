using OpenCvSharp;
using TireRecognition.Domain;

namespace TireRecognition.Infrastructure;

public class CvImageDataHandle : ImageDataHandle, IDisposable
{
    public override ImageDimensions Dimensions => new() { Width = Data.Cols, Height = Data.Rows };
    public override byte[] Bytes => Data.ToBytes();
    public Mat Data { get; }

    public CvImageDataHandle(Mat data)
    {
        Data = data;
    }

    public void Dispose()
    {
        Data.Dispose();
    }
}