using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Scriptum.Models;

namespace Scriptum.Services;

public sealed class QwenVisionTranscriptionProvider : ITranscriptionProvider
{
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private readonly QwenVisionTranscriptionOptions _options;

    public QwenVisionTranscriptionProvider(QwenVisionTranscriptionOptions options)
    {
        _options = options;
    }

    public string Name => "Qwen vision transcription";

    public async Task<TranscriptionResult> TranscribeAsync(
        NotebookPage page,
        PageImagePreprocessingResult preprocessingResult,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_options.Enabled)
        {
            throw new NotSupportedException(
                "Qwen vision transcription is present as a disabled provider shell. Use the mock provider until the real API client is implemented.");
        }

        if (string.IsNullOrWhiteSpace(preprocessingResult.PreparedImagePath)
            || !File.Exists(preprocessingResult.PreparedImagePath))
        {
            throw new FileNotFoundException(
                "The prepared notebook page image was not found. Import the page again or rerun preprocessing before transcription.",
                preprocessingResult.PreparedImagePath);
        }

        if (string.IsNullOrWhiteSpace(_options.Endpoint))
        {
            throw new InvalidOperationException(
                "Set SCRIPTUM_QWEN_ENDPOINT to an OpenAI-compatible Qwen chat completions endpoint before transcription.");
        }

        var apiKey = Environment.GetEnvironmentVariable(_options.ApiKeyEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                $"Set {_options.ApiKeyEnvironmentVariable} before enabling Qwen vision transcription.");
        }

        var imageBytes = await File.ReadAllBytesAsync(preprocessingResult.PreparedImagePath, cancellationToken);
        var request = new QwenChatCompletionRequest
        {
            Model = _options.Model,
            MaxTokens = _options.MaxOutputTokens,
            Messages =
            [
                new QwenChatMessage
                {
                    Role = "system",
                    Content =
                    [
                        QwenContentBlock.FromText(
                            "You transcribe real handwritten notebook pages. Return only the readable text from the page. Preserve line breaks, headings, bullets, numbering, code-like snippets, and uncertain words using [unclear]. Do not summarize or invent missing content.")
                    ]
                },
                new QwenChatMessage
                {
                    Role = "user",
                    Content =
                    [
                        QwenContentBlock.FromText("Transcribe this handwritten notebook page into editable text."),
                        QwenContentBlock.FromImageUrl(BuildDataUrl(preprocessingResult.PreparedImagePath, imageBytes))
                    ]
                }
            ]
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, NormalizeEndpoint(_options.Endpoint));
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        httpRequest.Content = new StringContent(
            JsonSerializer.Serialize(request, JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await HttpClient.SendAsync(httpRequest, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Qwen transcription failed with HTTP {(int)response.StatusCode}: {TrimForStatus(responseText)}");
        }

        var rawText = ExtractAssistantText(responseText);
        if (string.IsNullOrWhiteSpace(rawText))
        {
            throw new InvalidOperationException("Qwen transcription returned an empty response.");
        }

        return new TranscriptionResult
        {
            ProviderName = $"{Name} ({_options.Model})",
            RawText = rawText.Trim()
        };
    }

    private static Uri NormalizeEndpoint(string endpoint)
    {
        var trimmed = endpoint.Trim().TrimEnd('/');
        if (trimmed.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
        {
            return new Uri(trimmed);
        }

        return new Uri($"{trimmed}/chat/completions");
    }

    private static string BuildDataUrl(string path, byte[] imageBytes)
    {
        var mimeType = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".bmp" => "image/bmp",
            ".gif" => "image/gif",
            ".tif" or ".tiff" => "image/tiff",
            _ => "application/octet-stream"
        };

        return $"data:{mimeType};base64,{Convert.ToBase64String(imageBytes)}";
    }

    private static string ExtractAssistantText(string responseText)
    {
        using var document = JsonDocument.Parse(responseText);
        if (!document.RootElement.TryGetProperty("choices", out var choices)
            || choices.ValueKind != JsonValueKind.Array
            || choices.GetArrayLength() == 0)
        {
            return string.Empty;
        }

        var firstChoice = choices[0];
        if (!firstChoice.TryGetProperty("message", out var message)
            || !message.TryGetProperty("content", out var content))
        {
            return string.Empty;
        }

        if (content.ValueKind == JsonValueKind.String)
        {
            return content.GetString() ?? string.Empty;
        }

        if (content.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var item in content.EnumerateArray())
        {
            if (item.TryGetProperty("text", out var text)
                && text.ValueKind == JsonValueKind.String)
            {
                builder.AppendLine(text.GetString());
            }
        }

        return builder.ToString();
    }

    private static string TrimForStatus(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "empty response";
        }

        return value.Length <= 500 ? value : $"{value[..500]}...";
    }

    private sealed class QwenChatCompletionRequest
    {
        public string Model { get; init; } = string.Empty;

        public IReadOnlyList<QwenChatMessage> Messages { get; init; } = [];

        public int MaxTokens { get; init; }
    }

    private sealed class QwenChatMessage
    {
        public string Role { get; init; } = string.Empty;

        public IReadOnlyList<QwenContentBlock> Content { get; init; } = [];
    }

    private sealed class QwenContentBlock
    {
        public string Type { get; init; } = string.Empty;

        public string? Text { get; init; }

        public QwenImageUrl? ImageUrl { get; init; }

        public static QwenContentBlock FromText(string text) => new()
        {
            Type = "text",
            Text = text
        };

        public static QwenContentBlock FromImageUrl(string url) => new()
        {
            Type = "image_url",
            ImageUrl = new QwenImageUrl
            {
                Url = url
            }
        };
    }

    private sealed class QwenImageUrl
    {
        public string Url { get; init; } = string.Empty;
    }
}
