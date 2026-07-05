using Scriptum.Models;

namespace Scriptum.Services;

public interface ITranscriptionProvider
{
    string Name { get; }

    Task<TranscriptionResult> TranscribeAsync(
        NotebookPage page,
        PageImagePreprocessingResult preprocessingResult,
        CancellationToken cancellationToken = default);
}
