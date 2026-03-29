using OpenCvSharp;
using TireRecognition.Domain.Preprocessing;

namespace TireRecognition.Infrastructure;

public class CvImageDataHandle : ImageDataHandle
{
    public override ImageDimensions Dimensions => new(Width: Data.Cols, Height: Data.Rows);
    public override byte[] Bytes => Data.ToBytes();
    public Mat Data { get; set; }

    public CvImageDataHandle(Mat data)
    {
        Data = data;
    }

    public override ImageDataHandle Clone() => new CvImageDataHandle(Data.Clone());

    public override void Dispose() => Data.Dispose();
}