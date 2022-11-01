/*
	SwfDotNet is an open source library for writing and reading 
	Macromedia Flash (SWF) bytecode.
	Copyright (C) 2005 Olivier Carpentier - Adelina foundation
	see Licence.cs for GPL full text!
		
	SwfDotNet.IO uses a part of the open source library SwfOp actionscript 
	byte code management, writted by Florian Kr�sch, Copyright (C) 2004 .
	
	This library is free software; you can redistribute it and/or
	modify it under the terms of the GNU General Public
	License as published by the Free Software Foundation; either
	version 2.1 of the License, or (at your option) any later version.
	
	This library is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
	General Public License for more details.
	
	You should have received a copy of the GNU General Public
	License along with this library; if not, write to the Free Software
	Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using System.Collections;
using System.Xml;
using SWF.NET.Utils;

namespace SWF.NET.Tags.Types;

/// <summary>
///     GlyphEntry
/// </summary>
public class GlyphEntry : ISwfSerializer
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="GlyphEntry" /> instance.
    /// </summary>
    public GlyphEntry()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="GlyphEntry" /> instance.
    /// </summary>
    /// <param name="glyphIndex">Glyph index.</param>
    /// <param name="glyphAdvance">Glyph advance.</param>
    public GlyphEntry(uint glyphIndex, int glyphAdvance)
    {
        this.GlyphIndex = glyphIndex;
        this.GlyphAdvance = glyphAdvance;
    }

    /// <summary>
    ///     Creates a new <see cref="GlyphEntry" /> instance.
    /// </summary>
    /// <param name="character">Character.</param>
    public GlyphEntry(char character) =>
        GlyphCharacter = character;

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the glyph character.
    /// </summary>
    /// <value></value>
    public char GlyphCharacter { get; set; } = '\0';

    /// <summary>
    ///     Gets or sets the glyph index.
    /// </summary>
    public uint GlyphIndex { get; set; }

    /// <summary>
    ///     Gets or sets the glyph advance.
    /// </summary>
    public int GlyphAdvance { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public void ReadData(BufferedBinaryReader binaryReader)
    {
        GlyphIndex = binaryReader.ReadUBits(TextRecordCollection.GLYPH_BITS);
        GlyphAdvance = binaryReader.ReadSBits(TextRecordCollection.ADVANCE_BITS);
    }

    /// <summary>
    ///     Gets the size of in bits number
    /// </summary>
    /// <returns></returns>
    public int GetBitsSizeOf()
    {
        var res = 0;
        res += TextRecordCollection.GLYPH_BITS;
        res += TextRecordCollection.ADVANCE_BITS;
        return res;
    }

    /// <summary>
    ///     Writes to.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BufferedBinaryWriter writer)
    {
        writer.WriteUBits(GlyphIndex, TextRecordCollection.GLYPH_BITS);
        writer.WriteSBits(GlyphAdvance, TextRecordCollection.ADVANCE_BITS);
    }

    /// <summary>
    ///     Serializes to the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("GlyphEntry");
        writer.WriteAttributeString("GlyphBits", TextRecordCollection.GLYPH_BITS.ToString());
        writer.WriteAttributeString("GlyphIndex", GlyphIndex.ToString());
        writer.WriteAttributeString("AdvanceBits", TextRecordCollection.ADVANCE_BITS.ToString());
        writer.WriteAttributeString("GlyphAdvance", GlyphAdvance.ToString());
        writer.WriteEndElement();
    }

    #endregion
}

/// <summary>
///     GlyphEntryCollection
/// </summary>
public class GlyphEntryCollection : CollectionBase
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="GlyphEntryCollection" /> instance.
    /// </summary>
    public GlyphEntryCollection()
    {
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf()
    {
        var res = 0;

        var glyphs = GetEnumerator();
        while (glyphs.MoveNext())
            res += ((GlyphEntry)glyphs.Current).GetBitsSizeOf();

        return Convert.ToInt32(Math.Ceiling(res / 8.0));
    }

    #endregion

    #region Collection methods

    /// <summary>
    ///     Adds the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public GlyphEntry Add(GlyphEntry value)
    {
        List.Add(value);
        return value;
    }

    /// <summary>
    ///     Adds the range.
    /// </summary>
    /// <param name="values">Values.</param>
    public void AddRange(GlyphEntry[] values)
    {
        foreach (var ip in values)
            Add(ip);
    }

    /// <summary>
    ///     Removes the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    public void Remove(GlyphEntry value)
    {
        if (List.Contains(value))
            List.Remove(value);
    }

    /// <summary>
    ///     Inserts the specified index.
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="value">Value.</param>
    public void Insert(int index, GlyphEntry value) =>
        List.Insert(index, value);

    /// <summary>
    ///     Containses the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public bool Contains(GlyphEntry value) =>
        List.Contains(value);

    /// <summary>
    ///     Gets or sets the <see cref="LineStyle" /> at the specified index.
    /// </summary>
    /// <value></value>
    public GlyphEntry this[int index]
    {
        get => (GlyphEntry)List[index];
        set => List[index] = value;
    }

    /// <summary>
    ///     Get the index of.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public int IndexOf(GlyphEntry value) =>
        List.IndexOf(value);

    #endregion
}

/// <summary>
///     Text Record class
/// </summary>
public class TextRecord : ISwfSerializer
{
    #region Members

    #endregion

    #region Ctor & Init

    /// <summary>
    ///     Creates a new <see cref="TextRecord" /> instance.
    /// </summary>
    public TextRecord() =>
        Init();

    /// <summary>
    ///     Inits this instance.
    /// </summary>
    protected void Init()
    {
        GlyphEntries = new GlyphEntryCollection();
        XOffset = short.MinValue;
        YOffset = short.MinValue;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the font id.
    /// </summary>
    public ushort FontId { get; set; }

    /// <summary>
    ///     Gets or sets the color of the text.
    /// </summary>
    public RGBColor TextColor { get; set; }

    /// <summary>
    ///     Gets or sets the X offset.
    /// </summary>
    public short XOffset { get; set; }

    /// <summary>
    ///     Gets or sets the Y offset.
    /// </summary>
    public short YOffset { get; set; }

    /// <summary>
    ///     Gets or sets the height of the text.
    /// </summary>
    public ushort TextHeight { get; set; }

    /// <summary>
    ///     Gets the glyph entries.
    /// </summary>
    public GlyphEntryCollection GlyphEntries { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    /// <param name="endOfRecordsFlag">End of records flag.</param>
    /// <param name="tagCodeEnum">Tag code enum.</param>
    public void ReadData(
        BufferedBinaryReader binaryReader,
        ref bool endOfRecordsFlag,
        TagCodeEnum tagCodeEnum)
    {
        binaryReader.SynchBits();
        var textRecordType = binaryReader.ReadBoolean();
        binaryReader.ReadUBits(3);

        var styleFlagsHasFont = binaryReader.ReadBoolean();
        var styleFlagsHasColor = binaryReader.ReadBoolean();
        var styleFlagsHasYOffset = binaryReader.ReadBoolean();
        var styleFlagsHasXOffset = binaryReader.ReadBoolean();

        if (textRecordType == false)
        {
            endOfRecordsFlag = true;
            return;
        }

        FontId = 0;
        if (styleFlagsHasFont)
            FontId = binaryReader.ReadUInt16();

        TextColor = null;
        if (styleFlagsHasColor)
        {
            if (tagCodeEnum == TagCodeEnum.DefineText2)
            {
                TextColor = new RGBA();
                TextColor.ReadData(binaryReader);
            }
            else
            {
                TextColor = new RGB();
                TextColor.ReadData(binaryReader);
            }
        }

        XOffset = 0;
        if (styleFlagsHasXOffset)
            XOffset = binaryReader.ReadInt16();

        YOffset = 0;
        if (styleFlagsHasYOffset)
            YOffset = binaryReader.ReadInt16();

        TextHeight = 0;
        if (styleFlagsHasFont)
            TextHeight = binaryReader.ReadUInt16();

        var glyphCount = binaryReader.ReadByte();
        if (glyphCount > 0)
        {
            if (GlyphEntries == null)
                GlyphEntries = new GlyphEntryCollection();
            else
                GlyphEntries.Clear();

            for (var i = 0; i < glyphCount; i++)
            {
                var glyphEntry = new GlyphEntry();
                glyphEntry.ReadData(binaryReader);
                GlyphEntries.Add(glyphEntry);
            }
        }
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>SIze of this object</returns>
    public int GetSizeOf()
    {
        var styleFlagsHasFont = HasFont();
        var styleFlagsHasColor = HasColor();
        var styleFlagsHasYOffset = HasYOffset();
        var styleFlagsHasXOffset = HasXOffset();

        var res = 1;
        if (styleFlagsHasFont)
            res += 2;
        if (styleFlagsHasColor)
            res += TextColor.GetSizeOf();
        if (styleFlagsHasXOffset)
            res += 2;
        if (styleFlagsHasYOffset)
            res += 2;
        if (styleFlagsHasFont)
            res += 2;
        res++;
        if (GlyphEntries != null)
            res += GlyphEntries.GetSizeOf();
        return res;
    }

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BufferedBinaryWriter writer)
    {
        writer.SynchBits();

        writer.WriteBoolean(true);
        writer.WriteUBits(0, 3);

        var styleFlagsHasFont = HasFont();
        var styleFlagsHasColor = HasColor();
        var styleFlagsHasYOffset = HasYOffset();
        var styleFlagsHasXOffset = HasXOffset();

        writer.WriteBoolean(styleFlagsHasFont);
        writer.WriteBoolean(styleFlagsHasColor);
        writer.WriteBoolean(styleFlagsHasYOffset);
        writer.WriteBoolean(styleFlagsHasXOffset);

        if (styleFlagsHasFont)
            writer.Write(FontId);

        if (styleFlagsHasColor)
            TextColor.WriteTo(writer);
        if (styleFlagsHasXOffset)
            writer.Write(XOffset);
        if (styleFlagsHasYOffset)
            writer.Write(YOffset);
        if (styleFlagsHasFont)
            writer.Write(TextHeight);
        writer.Write((byte)GlyphEntries.Count);

        if (GlyphEntries != null)
        {
            var glyphs = GlyphEntries.GetEnumerator();
            while (glyphs.MoveNext())
                ((GlyphEntry)glyphs.Current).WriteTo(writer);
        }
    }

    /// <summary>
    ///     Serializes with the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("TextRecord");
        writer.WriteElementString("StyleFlagsHasFont", HasFont().ToString());
        writer.WriteElementString("StyleFlagsHasColor", HasColor().ToString());
        writer.WriteElementString("StyleFlagsHasYOffset", HasYOffset().ToString());
        writer.WriteElementString("StyleFlagsHasXOffset", HasXOffset().ToString());

        if (HasFont())
            writer.WriteElementString("FontId", FontId.ToString());

        if (HasColor())
            TextColor.Serialize(writer);

        if (HasXOffset())
            writer.WriteElementString("XOffset", XOffset.ToString());

        if (HasYOffset())
            writer.WriteElementString("YOffset", YOffset.ToString());

        if (HasFont())
            writer.WriteElementString("TextHeight", TextHeight.ToString());

        if (GlyphEntries != null)
        {
            writer.WriteElementString("GlyphCount", GlyphEntries.Count.ToString());
            writer.WriteStartElement("GlyphEntries");

            var glyphs = GlyphEntries.GetEnumerator();
            while (glyphs.MoveNext())
                ((GlyphEntry)glyphs.Current).Serialize(writer);

            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    /// <summary>
    ///     Determines whether this instance has color.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this instance has color; otherwise, <c>false</c>.
    /// </returns>
    private bool HasColor() =>
        TextColor != null;

    /// <summary>
    ///     Determines whether this instance has font.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this instance has font; otherwise, <c>false</c>.
    /// </returns>
    private bool HasFont() =>
        FontId != 0 && TextHeight != 0;

    /// <summary>
    ///     Determines whether [has X offset].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [has X offset]; otherwise, <c>false</c>.
    /// </returns>
    private bool HasXOffset() =>
        XOffset != short.MinValue;

    /// <summary>
    ///     Determines whether [has Y offset].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [has Y offset]; otherwise, <c>false</c>.
    /// </returns>
    private bool HasYOffset() =>
        YOffset != short.MinValue;

    /// <summary>
    ///     Determines whether this instance has style.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this instance has style; otherwise, <c>false</c>.
    /// </returns>
    private bool HasStyle() =>
        HasFont() || HasColor() || HasXOffset() || HasYOffset();

    /// <summary>
    ///     Gets the glyph bits.
    /// </summary>
    /// <returns></returns>
    public byte GetGlyphBits()
    {
        byte numberOfBits = 0;

        var glyphs = GlyphEntries.GetEnumerator();
        while (glyphs.MoveNext())
        {
            var numBits = BufferedBinaryWriter.GetNumBits(((GlyphEntry)glyphs.Current).GlyphIndex);
            numberOfBits = (byte)Math.Max(numberOfBits, numBits);
        }

        return numberOfBits;
    }

    /// <summary>
    ///     Gets the advance bits.
    /// </summary>
    /// <returns></returns>
    public byte GetAdvanceBits()
    {
        byte numberOfBits = 0;

        var glyphs = GlyphEntries.GetEnumerator();
        while (glyphs.MoveNext())
        {
            var numBits = BufferedBinaryWriter.GetNumBits(((GlyphEntry)glyphs.Current).GlyphAdvance);
            numberOfBits = (byte)Math.Max(numberOfBits, numBits);
        }

        return numberOfBits;
    }

    #endregion
}

/// <summary>
///     TextRecordCollection class
/// </summary>
public class TextRecordCollection : CollectionBase
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="TextRecordCollection" /> instance.
    /// </summary>
    public TextRecordCollection()
    {
    }

    #endregion

    #region Static Members

    /// <summary>
    ///     Glyph bits number
    /// </summary>
    public static byte GLYPH_BITS = 0;

    /// <summary>
    ///     Advance bits number
    /// </summary>
    public static byte ADVANCE_BITS = 0;

    #endregion

    #region Collection methods

    /// <summary>
    ///     Adds the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public TextRecord Add(TextRecord value)
    {
        List.Add(value);
        return value;
    }

    /// <summary>
    ///     Adds the range.
    /// </summary>
    /// <param name="values">Values.</param>
    public void AddRange(TextRecord[] values)
    {
        foreach (var ip in values)
            Add(ip);
    }

    /// <summary>
    ///     Removes the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    public void Remove(TextRecord value)
    {
        if (List.Contains(value))
            List.Remove(value);
    }

    /// <summary>
    ///     Inserts the specified index.
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="value">Value.</param>
    public void Insert(int index, TextRecord value) =>
        List.Insert(index, value);

    /// <summary>
    ///     Containses the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public bool Contains(TextRecord value) =>
        List.Contains(value);

    /// <summary>
    ///     Gets or sets the <see cref="TextRecord" /> at the specified index.
    /// </summary>
    /// <value></value>
    public TextRecord this[int index]
    {
        get => (TextRecord)List[index];
        set => List[index] = value;
    }

    /// <summary>
    ///     Get the index of.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public int IndexOf(TextRecord value) =>
        List.IndexOf(value);

    #endregion
}