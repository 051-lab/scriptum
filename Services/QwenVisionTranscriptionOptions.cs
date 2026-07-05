namespace Scriptum.Services;

public sealed class QwenVisionTranscriptionOptions
{
    public bool Enabled { get; set; }

    public string Model { get; set; } = "qwen-vl";

    public string? Endpoint { get; set; }

    public string ApiKeyEnvironmentVariable { get; set; } = "QWEN_API_KEY";

    public int MaxOutputTokens { get; set; } = 4096;
}
