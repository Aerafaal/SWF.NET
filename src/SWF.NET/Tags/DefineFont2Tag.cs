/*
	SwfDotNet is an open source library for writing and reading 
	Macromedia Flash (SWF) bytecode.
	Copyright (C) 2005 Olivier Carpentier - Adelina foundation
	see Licence.cs for GPL full text!
		
	SwfDotNet.IO uses a part of the open source library SwfOp actionscript 
	byte code management, writted by Florian Krüsch, Copyright (C) 2004 .
	
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

using System.IO;
using System.Xml;
using SWF.NET.Tags.Types;
using SWF.NET.Utils;

namespace SWF.NET.Tags;

/// <summary>
///     DefineFont2Tag defines the shapes and layout of the glyphs
///     used in a font.
/// </summary>
/// <remarks>
///     <p>
///         It extends the functionality provided by the FSDefineFont class by:
///         <ul>
///             <li>Allowing more than 65535 glyphs in a particular font.</li>
///             <li>Including the functionality provided by the DefineFontInfoTag class.</li>
///             <li>Specifying ascent, descent and leading layout information for the font.</li>
///             <li>Specifying advances for each glyph.</li>
///             <li>Specifying bounding rectangles for each glyph.</li>
///             <li>Specifying kerning pairs defining the distance between pairs of glyphs.</li>
///         </ul>
///     </p>
///     <p>
///         This tag was introduced in Flash 2. Support for spoken languages was added
///         in Flash 6. Support for small point size fonts was added in Flash 7.
///     </p>
/// </remarks>
public class DefineFont2Tag : BaseTag, DefineTag, IDefineFont
{
    #region Members

    #endregion

    #region Ctor & Init

    /// <summary>
    ///     Creates a new <see cref="DefineFont2Tag" /> instance.
    /// </summary>
    public DefineFont2Tag() =>
        Init();

    /// <summary>
    ///     Inits this instance.
    /// </summary>
    protected void Init()
    {
        GlyphShapesTable = new GlyphShapesTable();
        BoundsTable = new RectCollection();
        KerningTable = new KerningRecordCollection();
        AdvanceTable = new ShortCollection();
        _tagCode = (int)TagCodeEnum.DefineFont2;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the language code.
    /// </summary>
    /// <value></value>
    public LanguageCode LanguageCode { get; set; }

    /// <summary>
    ///     Gets or sets the font leading height.
    /// </summary>
    /// <value></value>
    public short Leading { get; set; }

    /// <summary>
    ///     Gets or sets the font descender height.
    /// </summary>
    /// <value></value>
    public short Descent { get; set; }

    /// <summary>
    ///     Gets or sets the font ascender height.
    /// </summary>
    /// <value></value>
    public short Ascent { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="DefineFont2Tag" /> is ANSI encoded.
    /// </summary>
    /// <value>
    ///     <c>true</c> if ANSI; otherwise, <c>false</c>.
    /// </value>
    public bool ANSI { get; set; }

    /// <summary>
    ///     Gets or sets if text is small. Character
    ///     glyphs are aligned on pixel boundaries for dynamic and
    ///     input text.
    /// </summary>
    /// <value>
    ///     <c>true</c> if [small text]; otherwise, <c>false</c>.
    /// </value>
    public bool SmallText { get; set; }

    /// <summary>
    ///     Gets or sets if Shift JIS encoding is on.
    /// </summary>
    /// <value>
    ///     <c>true</c> if [shift JIS]; otherwise, <c>false</c>.
    /// </value>
    public bool ShiftJIS { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="DefineFont2Tag" /> is italic.
    /// </summary>
    /// <value>
    ///     <c>true</c> if italic; otherwise, <c>false</c>.
    /// </value>
    public bool Italic { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="DefineFont2Tag" /> is bold.
    /// </summary>
    /// <value>
    ///     <c>true</c> if bold; otherwise, <c>false</c>.
    /// </value>
    public bool Bold { get; set; }

    /// <summary>
    ///     see <see cref="DefineTag" />
    /// </summary>
    public ushort CharacterId { get; set; }

    /// <summary>
    ///     Gets or sets the glyph shape table.
    /// </summary>
    public GlyphShapesTable GlyphShapesTable { get; private set; }

    /// <summary>
    ///     Gets the bounds table.
    ///     Not used through the version 7, but must be present.
    /// </summary>
    /// <value></value>
    public RectCollection BoundsTable { get; private set; }

    /// <summary>
    ///     Gets the kerning table.
    /// </summary>
    /// <value></value>
    public KerningRecordCollection KerningTable { get; private set; }

    /// <summary>
    ///     Gets or sets the name of the font.
    /// </summary>
    /// <value></value>
    public string FontName { get; set; }

    /// <summary>
    ///     Gets the advance table to be used for each glyph
    ///     in dynamic glyph text.
    /// </summary>
    /// <value></value>
    public ShortCollection AdvanceTable { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void ReadData(byte version, BufferedBinaryReader binaryReader)
    {
        var rh = new RecordHeader();
        rh.ReadData(binaryReader);

        CharacterId = binaryReader.ReadUInt16();

        var fontFlagsHasLayout = binaryReader.ReadBoolean();
        ShiftJIS = binaryReader.ReadBoolean();
        SmallText = binaryReader.ReadBoolean();
        ANSI = binaryReader.ReadBoolean();
        var fontFlagsWideOffsets = binaryReader.ReadBoolean();
        var fontFlagsWideCodes = binaryReader.ReadBoolean();
        Italic = binaryReader.ReadBoolean();
        Bold = binaryReader.ReadBoolean();
        LanguageCode = (LanguageCode)binaryReader.ReadByte();
        var fontNameLength = binaryReader.ReadByte();

        FontName = binaryReader.ReadString(fontNameLength);

        var numGlyphs = binaryReader.ReadUInt16();

        if (numGlyphs > 0)
        {
            var offsetTable = new uint[numGlyphs];
            for (var i = 0; i < numGlyphs; i++)
                if (fontFlagsWideOffsets)
                    offsetTable[i] = binaryReader.ReadUInt32();
                else
                    offsetTable[i] = binaryReader.ReadUInt16();
        }

        uint codeTableOffset = 0;
        if (fontFlagsWideOffsets)
            codeTableOffset = binaryReader.ReadUInt32();
        else
            codeTableOffset = binaryReader.ReadUInt16();

        if (numGlyphs > 0)
        {
            GlyphShapesTable.IsWideCodes = fontFlagsWideCodes;
            GlyphShapesTable.ReadData(binaryReader, numGlyphs);
        }

        if (fontFlagsHasLayout)
        {
            Ascent = binaryReader.ReadInt16();
            Descent = binaryReader.ReadInt16();
            Leading = binaryReader.ReadInt16();

            if (numGlyphs > 0)
            {
                AdvanceTable.ReadData(binaryReader, numGlyphs);
                BoundsTable.ReadData(binaryReader, numGlyphs);
                KerningTable.ReadData(binaryReader, fontFlagsWideCodes);
            }
        }
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf()
    {
        var fontFlagsWideOffsets = HasWideOffsets();
        var res = 5;
        if (FontName != null)
            res += FontName.Length + 1;
        res += 2;

        var numGlyphs = GetNumGlyphs();

        if (numGlyphs > 0)
        {
            if (fontFlagsWideOffsets)
                res += numGlyphs * 4;
            else
                res += numGlyphs * 2;
        }

        if (fontFlagsWideOffsets)
            res += 4;
        else
            res += 2;

        if (GlyphShapesTable != null)
            res += GlyphShapesTable.GetSizeOf();

        if (HasLayoutInfo())
        {
            res += 6;
            res += AdvanceTable.GetSizeOf();
            res += BoundsTable.GetSizeOf();
            res += KerningTable.GetSizeOf();
        }

        return res;
    }

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void UpdateData(byte version)
    {
        if (version < 2)
            return;

        var fontFlagsWideOffsets = HasWideOffsets();
        var fontFlagsHasLayout = HasLayoutInfo();
        var fontFlagsWideCodes = HasWideCodes(version);
        if (GlyphShapesTable != null)
            GlyphShapesTable.IsWideCodes = fontFlagsWideCodes;

        var m = new MemoryStream();
        var w = new BufferedBinaryWriter(m);

        var rh = new RecordHeader(TagCode, GetSizeOf());
        rh.WriteTo(w);

        w.Write(CharacterId);

        w.WriteBoolean(fontFlagsHasLayout);
        w.WriteBoolean(ShiftJIS);
        w.WriteBoolean(SmallText);
        w.WriteBoolean(ANSI);
        w.WriteBoolean(fontFlagsWideOffsets);
        w.WriteBoolean(fontFlagsWideCodes);
        w.WriteBoolean(Italic);
        w.WriteBoolean(Bold);
        if (version >= 6)
            w.Write((byte)LanguageCode);
        else
            w.Write((byte)0);
        w.Write((byte)(FontName.Length + 1));
        w.WriteString(FontName);

        var numGlyph = GetNumGlyphs();
        w.Write((ushort)numGlyph);

        GlyphShapesTable.IsWideCodes = fontFlagsWideCodes;

        //Create the codetableoffset and offsettable
        var offsetTableSize = 0;
        if (fontFlagsWideOffsets)
            offsetTableSize = numGlyph * 4 + 4;
        else
            offsetTableSize = numGlyph * 2 + 2;

        var codes = GlyphShapesTable.GetOrderedCodes();
        var glyphsEnum = GlyphShapesTable.GetOrderedGlyphs(codes).GetEnumerator();
        var currentOffset = 0;
        for (var i = 0; glyphsEnum.MoveNext(); i++)
        {
            long offset = offsetTableSize + currentOffset;
            if (fontFlagsWideOffsets)
                w.Write((uint)offset);
            else
                w.Write((ushort)offset);

            var shapes = (ShapeRecordCollection)glyphsEnum.Current;
            var shapeSize = shapes.GetSizeOf();
            currentOffset += shapeSize;
        }

        if (fontFlagsWideOffsets)
            w.Write((uint)(offsetTableSize + currentOffset));
        else
            w.Write((ushort)(offsetTableSize + currentOffset));

        GlyphShapesTable.WriteTo(w);

        if (fontFlagsHasLayout)
        {
            w.Write(Ascent);
            w.Write(Descent);
            w.Write(Leading);

            if (numGlyph > 0)
            {
                AdvanceTable.WriteTo(w);
                BoundsTable.WriteTo(w);
                if (version >= 7)
                    w.Write((ushort)0);
                else
                    KerningTable.WriteTo(w);
            }
        }

        w.Flush();
        // write to data array
        _data = m.ToArray();
    }

    /// <summary>
    ///     Serializes with the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("DefineFont2Tag");
        writer.WriteAttributeString("FontId", CharacterId.ToString());

        var fontFlagsWideCodes = GlyphShapesTable.IsWideCodes;
        var fontFlagsWideOffsets = HasWideOffsets();
        var fontFlagsHasLayout = HasLayoutInfo();
        writer.WriteElementString("FontFlagsHasLayout", fontFlagsHasLayout.ToString());
        writer.WriteElementString("FontFlagsShiftJIS", ShiftJIS.ToString());
        writer.WriteElementString("FontFlagsANSI", ANSI.ToString());
        writer.WriteElementString("FontFlagsWideOffsets", fontFlagsWideOffsets.ToString());
        writer.WriteElementString("FontFlagsWideCodes", fontFlagsWideCodes.ToString());
        writer.WriteElementString("FontFlagsItalic", Italic.ToString());
        writer.WriteElementString("FontFlagsBold", Bold.ToString());
        writer.WriteElementString("LanguageCode", LanguageCode.ToString());
        writer.WriteElementString("FontNameLength", FontName.Length.ToString());
        writer.WriteElementString("FontName", FontName);

        //if (offsetTable != null)
        //	writer.WriteElementString("OffsetTableLenght", offsetTable.Length.ToString());
        //writer.WriteElementString("CodeTableOffset", codeTableOffset.ToString());

        if (GlyphShapesTable != null)
            GlyphShapesTable.Serialize(writer);

        if (fontFlagsHasLayout)
        {
            writer.WriteElementString("FontAscent", Ascent.ToString());
            writer.WriteElementString("FontDescent", Descent.ToString());
            writer.WriteElementString("FontLeading", Leading.ToString());
            writer.WriteElementString("FontAdvanceTable", AdvanceTable.Count.ToString());
            BoundsTable.Serialize(writer);
            KerningTable.Serialize(writer);
        }

        writer.WriteEndElement();
    }

    /// <summary>
    ///     Gets the num glyphs.
    /// </summary>
    /// <returns></returns>
    private int GetNumGlyphs()
    {
        var numGlyph = 0;
        if (GlyphShapesTable != null)
            numGlyph = GlyphShapesTable.Count;
        else if (AdvanceTable != null)
            numGlyph = AdvanceTable.Count;
        return numGlyph;
    }

    /// <summary>
    ///     Determines whether has wide codes.
    /// </summary>
    /// <param name="version">Version.</param>
    /// <returns>
    ///     <c>true</c> if has wide codes; otherwise, <c>false</c>.
    /// </returns>
    private bool HasWideCodes(byte version)
    {
        if (version >= 6)
            return true;
        return !ANSI;
    }

    /// <summary>
    ///     Determines whether [has layout info].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [has layout info]; otherwise, <c>false</c>.
    /// </returns>
    private bool HasLayoutInfo()
    {
        var layout = false;

        layout = layout || Ascent != 0;
        layout = layout || Descent != 0;
        layout = layout || Leading != 0;
        layout = layout || (AdvanceTable != null && AdvanceTable.Count > 0);
        layout = layout || (BoundsTable != null && BoundsTable.Count > 0);
        layout = layout || (KerningTable != null && KerningTable.Count > 0);
        return layout;
    }

    /// <summary>
    ///     Determines whether [has wide offsets].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [has wide offsets]; otherwise, <c>false</c>.
    /// </returns>
    private bool HasWideOffsets()
    {
        var wideOffsets = false;

        var glyphLength = 0;
        glyphLength += GlyphShapesTable.GetGlyphsSizeOf();

        if (GlyphShapesTable.Count * 2 + glyphLength > 65535)
            wideOffsets = true;

        return wideOffsets;
    }

    #endregion
}