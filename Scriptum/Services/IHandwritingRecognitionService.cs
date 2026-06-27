using Scriptum.Models.DTOs;

namespace Scriptum.Services;

/// <summary>
/// Service interface for handwriting recognition using Qwen-VL.
/// </summary>
public interface IHandwritingRecognitionService
{
    Task<string> RecognizeHandwritingAsync(byte[] imageData);
    Task<string> RecognizeHandwritingFromStrokesAsync(List<StrokeData> strokes, float canvasWidth, float canvasHeight);
    Task<string> RecognizeHandwritingAsync(List<List<Windows.Foundation.PointF>> strokes);
}
