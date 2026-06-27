using Microsoft.Graphics.Canvas;
using OpenCvSharp;
using Scriptum.Models.DTOs;
using Windows.Storage.Streams;

namespace Scriptum.Services;

/// <summary>
/// Service for rendering strokes to an image using Win2D.
/// </summary>
public class StrokeToImageRenderer
{
    /// <summary>
    /// Renders a list of strokes to a PNG image byte array.
    /// </summary>
    public async Task<byte[]> RenderStrokesAsync(List<StrokeData> strokes, float width, float height)
    {
        var device = CanvasDevice.GetSharedDevice();
        var offscreen = new CanvasRenderTarget(device, width, height, 96);

        using (var ds = offscreen.CreateDrawingSession())
        {
            ds.Clear(Windows.UI.Colors.White);

            foreach (var stroke in strokes)
            {
                if (stroke.Points.Count < 2) continue;

                var color = ParseColor(stroke.Color);
                ds.LineWidth = stroke.Thickness;

                // Draw lines between consecutive points
                for (int i = 1; i < stroke.Points.Count; i++)
                {
                    var start = new Windows.UI.Point(stroke.Points[i - 1].X, stroke.Points[i - 1].Y);
                    var end = new Windows.UI.Point(stroke.Points[i].X, stroke.Points[i].Y);
                    
                    if (stroke.IsHighlighter)
                    {
                        color.A = 128; // Semi-transparent for highlighter
                    }

                    ds.DrawLine(start, end, color);
                }
            }
        }

        // Convert to byte array
        using var stream = new InMemoryRandomAccessStream();
        await offscreen.SaveAsync(stream, CanvasBitmapFileFormat.Png);
        
        using var reader = new DataReader(stream.GetInputStreamAt(0));
        await reader.LoadAsync((uint)stream.Size);
        var bytes = new byte[stream.Size];
        reader.ReadBytes(bytes);
        
        return bytes;
    }

    private Windows.UI.Color ParseColor(string colorHex)
    {
        if (string.IsNullOrEmpty(colorHex) || !colorHex.StartsWith("#"))
        {
            return Windows.UI.Colors.Black;
        }

        try
        {
            var hex = colorHex.TrimStart('#');
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return Windows.UI.Color.FromArgb(255, r, g, b);
        }
        catch
        {
            return Windows.UI.Colors.Black;
        }
    }
}
