namespace Scriptum.Services;

public sealed class TranscriptionResult
{
    public string ProviderName { get; set; } = "Unknown";

    public string RawText { get; set; } = string.Empty;
}
