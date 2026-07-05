using Scriptum.Models;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;

namespace Scriptum.Services;

public sealed class WindowsOcrTranscriptionProvider : ITranscriptionProvider
{
    public string Name => "Windows OCR";

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

        var engine = OcrEngine.TryCreateFromUserProfileLanguages()
            ?? throw new InvalidOperationException("Windows OCR is not available for the current user language profile.");

        var bitmap = await LoadSoftwareBitmapAsync(preprocessingResult.PreparedImagePath, cancellationToken);
        if (bitmap.PixelWidth > OcrEngine.MaxImageDimension || bitmap.PixelHeight > OcrEngine.MaxImageDimension)
        {
            throw new InvalidOperationException(
                $"Windows OCR supports images up to {OcrEngine.MaxImageDimension}px per side. Crop or resize this page before transcription.");
        }

        var result = await engine.RecognizeAsync(bitmap).AsTask(cancellationToken);
        var rawText = BuildText(result);
        if (string.IsNullOrWhiteSpace(rawText))
        {
            rawText = "[Windows OCR did not detect readable text on this page.]";
        }

        return new TranscriptionResult
        {
            ProviderName = Name,
            RawText = rawText
        };
    }

    private static async Task<SoftwareBitmap> LoadSoftwareBitmapAsync(string imagePath, CancellationToken cancellationToken)
    {
        var file = await StorageFile.GetFileFromPathAsync(imagePath).AsTask(cancellationToken);
        using var stream = await file.OpenReadAsync().AsTask(cancellationToken);
        var decoder = await BitmapDecoder.CreateAsync(stream).AsTask(cancellationToken);
        return await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied)
            .AsTask(cancellationToken);
    }

    private static string BuildText(OcrResult result) => string.Join(
        Environment.NewLine,
        result.Lines
            .Select(line => line.Text)
            .Where(line => !string.IsNullOrWhiteSpace(line)));
}
