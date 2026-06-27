using MessagePack;
using Scriptum.Models.DTOs;

namespace Scriptum.Services;

/// <summary>
/// Handles serialization and deserialization of stroke data using MessagePack.
/// </summary>
public class StrokeSerializer
{
    /// <summary>
    /// Serializes a list of strokes to a byte array.
    /// </summary>
    public static byte[] Serialize(List<StrokeData> strokes)
    {
        return MessagePackSerializer.Serialize(strokes);
    }

    /// <summary>
    /// Deserializes a byte array to a list of strokes.
    /// </summary>
    public static List<StrokeData> Deserialize(byte[]? data)
    {
        if (data == null || data.Length == 0)
        {
            return new List<StrokeData>();
        }

        try
        {
            return MessagePackSerializer.Deserialize<List<StrokeData>>(data) ?? new List<StrokeData>();
        }
        catch
        {
            // Return empty list if deserialization fails
            return new List<StrokeData>();
        }
    }
}
