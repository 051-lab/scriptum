using Scriptum.Models;
using Scriptum.Models.DTOs;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace Scriptum.Services;

/// <summary>
/// Implementation of export functionality for notes.
/// Supports PNG and PDF export formats.
/// </summary>
public class ExportService : IExportService
{
    private readonly StrokeToImageRenderer _strokeRenderer;

    public ExportService()
    {
        _strokeRenderer = new StrokeToImageRenderer();
    }

    public async Task<byte[]> ExportToPngAsync(List<StrokeData> strokes, float canvasWidth, float canvasHeight)
    {
        if (strokes == null || strokes.Count == 0)
        {
            throw new ArgumentException("No strokes to export", nameof(strokes));
        }

        // Render strokes to image using existing renderer
        var pngBytes = await _strokeRenderer.RenderToPngAsync(strokes, canvasWidth, canvasHeight);
        return pngBytes;
    }

    public async Task<byte[]> ExportToPdfAsync(Note note, float canvasWidth, float canvasHeight)
    {
        // For PDF export, we'll create a simple PDF with the rendered image
        // This is a basic implementation - production would use a proper PDF library
        
        if (note.StrokeData == null || note.StrokeData.Length == 0)
        {
            throw new ArgumentException("No stroke data in note", nameof(note));
        }

        var strokes = StrokeSerializer.Deserialize(note.StrokeData);
        var pngBytes = await ExportToPngAsync(strokes, canvasWidth, canvasHeight);

        // Create a minimal PDF structure
        // Note: This is a simplified PDF - for production use a library like PdfSharp or iText
        var pdfContent = $@"%PDF-1.4
1 0 obj
<< /Type /Catalog /Pages 2 0 R >>
endobj
2 0 obj
<< /Type /Pages /Kids [3 0 R] /Count 1 >>
endobj
3 0 obj
<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {canvasWidth} {canvasHeight}] 
   /Contents 4 0 R /Resources << /XObject << /Img1 5 0 R >> >> >>
endobj
4 0 obj
<< /Length 61 >>
stream
q
{canvasWidth} 0 0 {canvasHeight} 0 0 cm
/Img1 Do
Q
endstream
endobj
5 0 obj
<< /Type /XObject /Subtype /Image /Width {(int)canvasWidth} /Height {(int)canvasHeight} 
   /ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode 
   /Length {pngBytes.Length} >>
stream
";

        var footer = @"
endstream
endobj
xref
0 6
0000000000 65535 f 
0000000009 00000 n 
0000000058 00000 n 
0000000117 00000 n 
0000000264 00000 n 
0000000373 00000 n 
trailer
<< /Size 6 /Root 1 0 R >>
startxref
%%EOF
";

        // Combine PDF parts (this is simplified - real implementation needs proper byte handling)
        var headerBytes = System.Text.Encoding.ASCII.GetBytes(pdfContent);
        var footerBytes = System.Text.Encoding.ASCII.GetBytes(footer);
        
        var pdfBytes = new byte[headerBytes.Length + pngBytes.Length + footerBytes.Length];
        Buffer.BlockCopy(headerBytes, 0, pdfBytes, 0, headerBytes.Length);
        Buffer.BlockCopy(pngBytes, 0, pdfBytes, headerBytes.Length, pngBytes.Length);
        Buffer.BlockCopy(footerBytes, 0, pdfBytes, headerBytes.Length + pngBytes.Length, footerBytes.Length);

        return pdfBytes;
    }
}
