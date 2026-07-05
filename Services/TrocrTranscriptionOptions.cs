namespace Scriptum.Services;

public sealed class TrocrTranscriptionOptions
{
    public string PythonExecutable { get; set; } = "python";

    public string ScriptPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "tools", "transcribe_trocr.py");

    public string Model { get; set; } = "microsoft/trocr-base-handwritten";

    public string Mode { get; set; } = "lines";

    public int TimeoutSeconds { get; set; } = 180;
}
