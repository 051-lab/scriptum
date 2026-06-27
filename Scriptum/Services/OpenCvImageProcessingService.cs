using OpenCvSharp;

namespace Scriptum.Services;

/// <summary>
/// OpenCV-based image processing service implementation.
/// </summary>
public class OpenCvImageProcessingService : IImageProcessingService
{
    public Task<byte[]> PreprocessImageAsync(byte[] imageData)
    {
        var result = ConvertToGrayscaleAsync(imageData).Result;
        result = ApplyThresholdAsync(result, 0.6).Result;
        return Task.FromResult(result);
    }

    public Task<byte[]> ConvertToGrayscaleAsync(byte[] imageData)
    {
        using var image = Cv2.ImDecode(imageData, ImreadModes.Color);
        if (image == null) throw new ArgumentException("Invalid image data");

        Cv2.CvtColor(image, image, ColorConversionCodes.BGR2GRAY);
        
        using var encoder = new ImageEncoder();
        var encoded = encoder.Encode(".png", image);
        return Task.FromResult(encoded);
    }

    public Task<byte[]> ApplyThresholdAsync(byte[] imageData, double threshold = 0.5)
    {
        using var image = Cv2.ImDecode(imageData, ImreadModes.GrayScale);
        if (image == null) throw new ArgumentException("Invalid image data");

        var threshValue = threshold * 255;
        Cv2.Threshold(image, image, threshValue, 255, ThresholdTypes.BinaryInv);
        
        using var encoder = new ImageEncoder();
        var encoded = encoder.Encode(".png", image);
        return Task.FromResult(encoded);
    }

    public Task<byte[]> DetectEdgesAsync(byte[] imageData)
    {
        using var image = Cv2.ImDecode(imageData, ImreadModes.GrayScale);
        if (image == null) throw new ArgumentException("Invalid image data");

        Cv2.Canny(image, image, 50, 150);
        
        using var encoder = new ImageEncoder();
        var encoded = encoder.Encode(".png", image);
        return Task.FromResult(encoded);
    }
}
