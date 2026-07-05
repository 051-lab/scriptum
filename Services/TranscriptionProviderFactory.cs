namespace Scriptum.Services;

public static class TranscriptionProviderFactory
{
    public static ITranscriptionProvider CreateDefault()
    {
        var provider = Environment.GetEnvironmentVariable("SCRIPTUM_TRANSCRIPTION_PROVIDER");
        if (string.Equals(provider, "windows-ocr", StringComparison.OrdinalIgnoreCase)
            || string.Equals(provider, "windows", StringComparison.OrdinalIgnoreCase))
        {
            return new WindowsOcrTranscriptionProvider();
        }

        var useQwen = string.Equals(provider, "qwen", StringComparison.OrdinalIgnoreCase)
            || string.Equals(
                Environment.GetEnvironmentVariable("SCRIPTUM_USE_QWEN_TRANSCRIPTION"),
                "true",
                StringComparison.OrdinalIgnoreCase);

        if (useQwen)
        {
            return new QwenVisionTranscriptionProvider(new QwenVisionTranscriptionOptions
            {
                Enabled = true,
                Endpoint = Environment.GetEnvironmentVariable("SCRIPTUM_QWEN_ENDPOINT"),
                Model = Environment.GetEnvironmentVariable("SCRIPTUM_QWEN_MODEL") ?? "qwen-vl",
                ApiKeyEnvironmentVariable = Environment.GetEnvironmentVariable("SCRIPTUM_QWEN_API_KEY_ENV")
                    ?? "QWEN_API_KEY"
            });
        }

        return new MockTranscriptionProvider();
    }
}
