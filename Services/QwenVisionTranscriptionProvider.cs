using Scriptum.Models;

namespace Scriptum.Services;

public sealed class QwenVisionTranscriptionProvider : ITranscriptionProvider
{
    private readonly QwenVisionTranscriptionOptions _options;

    public QwenVisionTranscriptionProvider(QwenVisionTranscriptionOptions options)
    {
        _options = options;
    }

    public string Name => "Qwen vision transcription";

    public Task<TranscriptionResult> TranscribeAsync(
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

        var apiKey = Environment.GetEnvironmentVariable(_options.ApiKeyEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                $"Set {_options.ApiKeyEnvironmentVariable} before enabling Qwen vision transcription.");
        }

        // Intended integration steps:
        // 1. Read the prepared page image.
        // 2. Send it to the configured Qwen vision endpoint/model.
        // 3. Return raw handwriting text only; correction remains user-owned.
        throw new NotImplementedException(
            "Qwen vision transcription is wired behind the provider boundary, but the real API client has not been implemented yet.");
    }
}
