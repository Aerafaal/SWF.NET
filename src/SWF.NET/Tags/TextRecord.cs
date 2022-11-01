using System.Collections;

namespace SWF.NET.Tags;

/// <summary>
///     Summary description for TextRecord.
/// </summary>
public class TextRecord
{
    public byte alpha;
    public byte blue;
    public BitArray flags;
    public ushort fontId;
    public byte glyphCount;
    public GlyphEntry[] glyphEntries;
    public byte green;
    public byte red;
    public ushort textHeight;
    public short xOffset;
    public short yOffset;
}

public struct GlyphEntry
{
    private BitArray glyphIndex;
    private BitArray glyphAdvance;
}