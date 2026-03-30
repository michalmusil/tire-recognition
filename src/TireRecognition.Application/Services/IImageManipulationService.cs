using TireRecognition.Domain.Preprocessing;

namespace TireRecognition.Application.Services;

public interface IImageManipulationService
{
    public ImageDataHandle GetImageHandleFromStream(Stream imageDataStream);
    public void ScaleToMaxDimension(ImageDataHandle image, int maxDimension);
    public void ApplyClahe(ImageDataHandle image);
    public void ApplyBilateralFilter(ImageDataHandle image);
    public void ApplyBitwiseNot(ImageDataHandle image);
    public void ProlongImageHorizontally(ImageDataHandle image, double prolongWidthRatio);
    public ImageDataHandle StackImagesVertically(List<ImageDataHandle> images);

    public List<ImageDataHandle> SliceImage(
        ImageDataHandle image,
        ImageDimensions sliceDimensions,
        double xOverlapRatio,
        double yOverlapRatio);
}