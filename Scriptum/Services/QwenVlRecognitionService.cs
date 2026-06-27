using System.Net.Http;
using System.Text;
using System.Text.Json;
using Scriptum.Helpers;

namespace Scriptum.Services;

/// <summary>
/// Qwen-VL API implementation for handwriting recognition.
/// </summary>
public class QwenVlRecognitionService : IHandwritingRecognitionService
{
    private readonly HttpClient _httpClient;
    private readonly string? _apiKey;
    private readonly string _apiEndpoint = "https://dashscope.aliyuncs.com/api/v1/services/aigc/multimodal-generation/generation";

    public QwenVlRecognitionService()
    {
        _apiKey = AppSettingsHelper.QwenApiKey;
        _httpClient = new HttpClient();
        if (!string.IsNullOrEmpty(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }
    }

    public async Task<string> RecognizeHandwritingAsync(byte[] imageData)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new InvalidOperationException("Qwen API key is not configured. Please set it in app settings.");
        }

        var base64Image = Convert.ToBase64String(imageData);
        var requestData = new
        {
            model = "qwen-vl-max",
            input = new
            {
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { text = "Please transcribe the handwritten text in this image. Return only the transcribed text without any additional commentary." },
                            new { image = $"data:image/png;base64,{base64Image}" }
                        }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync(_apiEndpoint, content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseJson);
        
        if (doc.RootElement.TryGetProperty("output", out var output) &&
            output.TryGetProperty("choices", out var choices) &&
            choices.GetArrayLength() > 0)
        {
            return choices[0].GetProperty("message").GetProperty("content").GetString() ?? string.Empty;
        }

        throw new Exception("Failed to parse Qwen-VL response");
    }

    public async Task<string> RecognizeHandwritingFromStrokesAsync(List<StrokeData> strokes, float canvasWidth, float canvasHeight)
    {
        var renderer = new StrokeToImageRenderer();
        var imageData = await renderer.RenderStrokesAsync(strokes, canvasWidth, canvasHeight);
        return await RecognizeHandwritingAsync(imageData);
    }

    public Task<string> RecognizeHandwritingAsync(List<List<Windows.Foundation.PointF>> strokes)
    {
        throw new NotImplementedException("Use RecognizeHandwritingFromStrokesAsync with StrokeData instead");
    }
}
