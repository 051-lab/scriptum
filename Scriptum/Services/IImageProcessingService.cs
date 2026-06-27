namespace Scriptum.Services;

/// <summary>
/// Service interface for image processing operations.
/// </summary>
public interface IImageProcessingService
{
    Task<byte[]> PreprocessImageAsync(byte[] imageData);
    Task<byte[]> ConvertToGrayscaleAsync(byte[] imageData);
    Task<byte[]> ApplyThresholdAsync(byte[] imageData, double threshold = 0.5);
    Task<byte[]> DetectEdgesAsync(byte[] imageData);
}
