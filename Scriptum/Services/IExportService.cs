using Scriptum.Models;

namespace Scriptum.Services;

/// <summary>
/// Service for exporting notes to various formats (PNG, PDF).
/// </summary>
public interface IExportService
{
    Task<byte[]> ExportToPngAsync(List<DTOs.StrokeData> strokes, float canvasWidth, float canvasHeight);
    Task<byte[]> ExportToPdfAsync(Note note, float canvasWidth, float canvasHeight);
}
