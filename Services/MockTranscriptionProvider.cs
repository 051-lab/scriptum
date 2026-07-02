using Scriptum.Models;

namespace Scriptum.Services;

public sealed class MockTranscriptionProvider : ITranscriptionProvider
{
    public string Name => "Mock local transcription";

    public Task<TranscriptionResult> TranscribeAsync(NotebookPage page, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var sourceName = string.IsNullOrWhiteSpace(page.OriginalFileName)
            ? page.Title
            : page.OriginalFileName;
        var imported = page.ImportedAt?.ToLocalTime().ToString("g") ?? "unknown import time";

        var rawText = $"""
            [Mock transcription]
            Source page: {sourceName}
            Imported: {imported}

            This placeholder represents the raw handwriting transcription that will be produced by an OCR or vision-model provider. Review it, move useful content into Corrected Text, and save the correction.
            """;

        return Task.FromResult(new TranscriptionResult
        {
            ProviderName = Name,
            RawText = rawText
        });
    }
}
