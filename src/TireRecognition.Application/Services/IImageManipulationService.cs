using TireRecognition.Domain.Preprocessing;

namespace TireRecognition.Application.Services;

public interface IImageManipulationService
{
    public ImageDataHandle GetImageHandleFromStream(Stream imageDataStream);
    public ImageDataHandle ScaleToMaxDimension(ImageDataHandle image, int maxDimension);
    public ImageDataHandle StackImagesVertically(List<ImageDataHandle> images);
    public ImageDataHandle ApplyClahe(ImageDataHandle image);
    public ImageDataHandle ApplyBilateralFilter(ImageDataHandle image);
    public ImageDataHandle ApplyBitwiseNot(ImageDataHandle image);
    public ImageDataHandle ProlongImageHorizontally(ImageDataHandle image, double prolongWidthRatio);

    public List<ImageDataHandle> SliceImage(
        ImageDataHandle image,
        ImageDimensions sliceDimensions,
        int xOverlap,
        int yOverlap);
}