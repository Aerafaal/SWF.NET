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
using System.IO;
using System.Xml;
using SWF.NET.Tags.Types;
using SWF.NET.Utils;

namespace SWF.NET.Tags;

/// <summary>
///     DefineText is the abstract class of the <see cref="DefineTextTag" />
///     and the <see cref="DefineText2Tag" /> object.
/// </summary>
public abstract class DefineText : BaseTag, DefineTag
{
    #region Init

    /// <summary>
    ///     Inits this instance.
    /// </summary>
    protected void Init()
    {
        TextRecords = new TextRecordCollection();
        Rect = new Rect();
        Matrix = new Matrix();
    }

    #endregion

    #region Members

    #endregion

    #region Properties

    /// <summary>
    ///     Gets the text records.
    /// </summary>
    /// <value></value>
    public TextRecordCollection TextRecords { get; private set; }

    /// <summary>
    ///     see <see cref="DefineTag" />
    /// </summary>
    public ushort CharacterId { get; set; }

    /// <summary>
    ///     Gets the rect.
    /// </summary>
    /// <value></value>
    public Rect Rect { get; private set; }

    /// <summary>
    ///     Gets the matrix.
    /// </summary>
    /// <value></value>
    public Matrix Matrix { get; private set; }

    /// <summary>
    ///     Gets the name of the tag.
    /// </summary>
    /// <value>The name of the tag.</value>
    protected abstract string TagName { get; }

    /// <summary>
    ///     Gets the version compatibility.
    /// </summary>
    /// <value>The version compatibility.</value>
    protected abstract int VersionCompatibility { get; }

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
        if (Rect == null)
            Rect = new Rect();
        Rect.ReadData(binaryReader);

        if (Matrix == null)
            Matrix = new Matrix();
        Matrix.ReadData(binaryReader);

        TextRecordCollection.GLYPH_BITS = binaryReader.ReadByte();
        TextRecordCollection.ADVANCE_BITS = binaryReader.ReadByte();

        if (TextRecords == null)
            TextRecords = new TextRecordCollection();
        else
            TextRecords.Clear();
        var endOfRecordsFlag = false;
        while (!endOfRecordsFlag)
        {
            var textRecord = new Types.TextRecord();
            textRecord.ReadData(binaryReader, ref endOfRecordsFlag, (TagCodeEnum)TagCode);
            if (!endOfRecordsFlag)
                TextRecords.Add(textRecord);
        }
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf()
    {
        TextRecordCollection.GLYPH_BITS = GetGlyphBits();
        TextRecordCollection.ADVANCE_BITS = GetAdvanceBits();

        var res = 2;
        if (Rect != null)
            res += Rect.GetSizeOf();
        if (Matrix != null)
            res += Matrix.GetSizeOf();
        res += 2;
        if (TextRecords != null)
        {
            var records = TextRecords.GetEnumerator();
            while (records.MoveNext())
                res += ((Types.TextRecord)records.Current).GetSizeOf();
        }

        res++;
        return res;
    }

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void UpdateData(byte version)
    {
        if (version < VersionCompatibility)
            return;

        var m = new MemoryStream();
        var w = new BufferedBinaryWriter(m);

        var rh = new RecordHeader(TagCode, GetSizeOf());
        rh.WriteTo(w);

        w.Write(CharacterId);
        if (Rect != null)
            Rect.WriteTo(w);
        if (Matrix != null)
            Matrix.WriteTo(w);

        w.Write(TextRecordCollection.GLYPH_BITS);
        w.Write(TextRecordCollection.ADVANCE_BITS);

        if (TextRecords != null)
        {
            var records = TextRecords.GetEnumerator();
            while (records.MoveNext())
                ((Types.TextRecord)records.Current).WriteTo(w);
        }

        w.Write((byte)0);

        w.Flush();
        // write to data array
        _data = m.ToArray();
    }

    /// <summary>
    ///     Gets the glyph bits.
    /// </summary>
    /// <returns></returns>
    private byte GetGlyphBits()
    {
        byte numberOfBits = 0;

        var records = TextRecords.GetEnumerator();
        while (records.MoveNext())
            numberOfBits = Math.Max(numberOfBits, ((Types.TextRecord)records.Current).GetGlyphBits());

        return numberOfBits;
    }

    /// <summary>
    ///     Gets the advance bits.
    /// </summary>
    /// <returns></returns>
    private byte GetAdvanceBits()
    {
        byte numberOfBits = 0;

        var records = TextRecords.GetEnumerator();
        while (records.MoveNext())
            numberOfBits = Math.Max(numberOfBits, ((Types.TextRecord)records.Current).GetAdvanceBits());

        return numberOfBits;
    }

    /// <summary>
    ///     Serializes with the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void Serialize(XmlWriter writer)
    {
        TextRecordCollection.GLYPH_BITS = GetGlyphBits();
        TextRecordCollection.ADVANCE_BITS = GetAdvanceBits();

        writer.WriteStartElement(TagName);

        writer.WriteAttributeString("CharacterId", CharacterId.ToString());
        if (Rect != null)
            Rect.Serialize(writer);
        if (Matrix != null)
            Matrix.Serialize(writer);

        writer.WriteElementString("GlyphBits", TextRecordCollection.GLYPH_BITS.ToString());
        writer.WriteElementString("AdvanceBits", TextRecordCollection.ADVANCE_BITS.ToString());

        writer.WriteStartElement("TextRecords");
        if (TextRecords != null)
        {
            var records = TextRecords.GetEnumerator();
            while (records.MoveNext())
                ((Types.TextRecord)records.Current).Serialize(writer);
        }

        writer.WriteEndElement();

        writer.WriteEndElement();
    }

    /// <summary>
    ///     Resolves method.
    ///     This method provides the way to update the textrecords glyph indexes
    ///     from Font object contained by the Swf Dictionnary.
    /// </summary>
    /// <param name="swf">SWF.</param>
    public override void Resolve(Swf swf)
    {
        var records = TextRecords.GetEnumerator();
        while (records.MoveNext())
        {
            var textRecord = (Types.TextRecord)records.Current;
            var glyphs = textRecord.GlyphEntries.GetEnumerator();
            while (glyphs.MoveNext())
            {
                var glyph = (Types.GlyphEntry)glyphs.Current;
                if (glyph.GlyphCharacter != '\0')
                {
                    object font = swf.Dictionary[textRecord.FontId];
                    if (font != null && font is DefineFont2Tag)
                    {
                        var glyphIndex = ((DefineFont2Tag)font).GlyphShapesTable.GetCharIndex(glyph.GlyphCharacter);
                        glyph.GlyphIndex = (uint)glyphIndex;
                    }
                    //TODO: For DefineFont
                }
            }
        }
    }

    #endregion
}