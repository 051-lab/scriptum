using System.Diagnostics;
using System.Text;
using Scriptum.Models;

namespace Scriptum.Services;

public sealed class TrocrTranscriptionProvider : ITranscriptionProvider
{
    private readonly TrocrTranscriptionOptions _options;

    public TrocrTranscriptionProvider(TrocrTranscriptionOptions options)
    {
        _options = options;
    }

    public string Name => $"TrOCR local ({_options.Model})";

    public async Task<TranscriptionResult> TranscribeAsync(
        NotebookPage page,
        PageImagePreprocessingResult preprocessingResult,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(preprocessingResult.PreparedImagePath)
            || !File.Exists(preprocessingResult.PreparedImagePath))
        {
            throw new FileNotFoundException(
                "The prepared notebook page image was not found. Import the page again or rerun preprocessing before transcription.",
                preprocessingResult.PreparedImagePath);
        }

        var scriptPath = ResolveScriptPath(_options.ScriptPath);
        if (string.IsNullOrWhiteSpace(scriptPath) || !File.Exists(scriptPath))
        {
            throw new FileNotFoundException(
                "The TrOCR helper script was not found. Set SCRIPTUM_TROCR_SCRIPT to tools/transcribe_trocr.py or another compatible script.",
                scriptPath);
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(30, _options.TimeoutSeconds)));

        var startInfo = new ProcessStartInfo
        {
            FileName = _options.PythonExecutable,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        startInfo.ArgumentList.Add(scriptPath);
        startInfo.ArgumentList.Add(preprocessingResult.PreparedImagePath);
        startInfo.ArgumentList.Add("--model");
        startInfo.ArgumentList.Add(_options.Model);
        startInfo.ArgumentList.Add("--mode");
        startInfo.ArgumentList.Add(_options.Mode);

        using var process = new Process
        {
            StartInfo = startInfo
        };

        var output = new StringBuilder();
        var errors = new StringBuilder();
        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                output.AppendLine(args.Data);
            }
        };
        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is not null)
            {
                errors.AppendLine(args.Data);
            }
        };

        if (!process.Start())
        {
            throw new InvalidOperationException("Unable to start the TrOCR helper process.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            TryKill(process);
            throw new TimeoutException($"TrOCR transcription timed out after {_options.TimeoutSeconds} seconds.");
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"TrOCR transcription failed: {TrimForStatus(errors.ToString())}");
        }

        var rawText = output.ToString().Trim();
        if (string.IsNullOrWhiteSpace(rawText))
        {
            rawText = "[TrOCR did not detect readable text on this page.]";
        }

        return new TranscriptionResult
        {
            ProviderName = Name,
            RawText = rawText
        };
    }

    private static string ResolveScriptPath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        var appBasePath = Path.Combine(AppContext.BaseDirectory, configuredPath);
        if (File.Exists(appBasePath))
        {
            return appBasePath;
        }

        return Path.GetFullPath(configuredPath);
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // The process may exit between timeout detection and cleanup.
        }
    }

    private static string TrimForStatus(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "empty error output";
        }

        var normalized = value.Trim();
        return normalized.Length <= 800 ? normalized : $"{normalized[..800]}...";
    }
}
