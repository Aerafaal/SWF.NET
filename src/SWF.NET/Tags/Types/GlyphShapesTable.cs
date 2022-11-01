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

using System.Collections;
using System.Xml;
using SWF.NET.Utils;

namespace SWF.NET.Tags.Types;

/// <summary>
///     GlyphShapesTable.
/// </summary>
public class GlyphShapesTable : Hashtable
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="GlyphShapesTable" /> instance.
    /// </summary>
    public GlyphShapesTable()
    {
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the <see cref="ShapeRecordCollection" /> with the specified character.
    /// </summary>
    /// <value></value>
    public ShapeRecordCollection this[char character]
    {
        get => base[character] as ShapeRecordCollection;
        set => base[character] = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether this instance is wide codes.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is wide codes; otherwise, <c>false</c>.
    /// </value>
    public bool IsWideCodes { get; set; } = false;

    #endregion

    #region Collection Methods

    /// <summary>
    ///     Adds the specified character.
    /// </summary>
    /// <param name="character">Character.</param>
    /// <param name="glyphShapes">Glyph shapes.</param>
    public void Add(char character, ShapeRecordCollection glyphShapes) =>
        base.Add(character, glyphShapes);

    /// <summary>
    ///     Removes the specified character.
    /// </summary>
    /// <param name="character">Character.</param>
    public void Remove(char character) =>
        base.Remove(character);

    /// <summary>
    ///     Test if containses the specified character.
    /// </summary>
    /// <param name="character">Character.</param>
    /// <returns></returns>
    public bool Contains(char character) =>
        base.Contains(character);

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf()
    {
        var res = 0;
        res += GetCodesSizeOf();
        res += GetGlyphsSizeOf();
        return res;
    }

    /// <summary>
    ///     Gets the codes size of.
    /// </summary>
    /// <returns></returns>
    public int GetCodesSizeOf()
    {
        if (IsWideCodes)
            return Keys.Count * 2;
        return Keys.Count;
    }

    /// <summary>
    ///     Gets the glyphs size of.
    /// </summary>
    /// <returns></returns>
    public int GetGlyphsSizeOf()
    {
        var res = 0;
        ShapeWithStyle.NumFillBits = 0;
        ShapeWithStyle.NumLineBits = 0;
        var glyphShapesCollections = Values.GetEnumerator();
        while (glyphShapesCollections.MoveNext())
        {
            var glyphs = (ShapeRecordCollection)glyphShapesCollections.Current;
            ShapeWithStyle.NumFillBits = 1;
            res += glyphs.GetSizeOf();
        }

        return res;
    }

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    /// <param name="numGlyphs">Num glyphs.</param>
    public void ReadData(BufferedBinaryReader binaryReader, ushort numGlyphs)
    {
        if (numGlyphs == 0)
            return;

        ShapeWithStyle.NumFillBits = 0;
        ShapeWithStyle.NumLineBits = 0;
        var shapes = new ShapeRecordCollection[numGlyphs];
        for (var i = 0; i < numGlyphs; i++)
        {
            var glyphShape = new ShapeRecordCollection();
            glyphShape.ReadData(binaryReader, ShapeType.None);
            shapes[i] = glyphShape;
        }

        for (var i = 0; i < numGlyphs; i++)
        {
            char c;
            if (IsWideCodes)
                c = (char)binaryReader.ReadUInt16();
            else
                c = (char)binaryReader.ReadByte();
            this[c] = shapes[i];
        }

        shapes = null;
    }

    /// <summary>
    ///     Gets the ordered glyphs.
    /// </summary>
    /// <param name="orderedCodes">Ordered codes.</param>
    public ShapeRecordCollection[] GetOrderedGlyphs(char[] orderedCodes)
    {
        var res = new ShapeRecordCollection[orderedCodes.Length];

        var codesEnum = orderedCodes.GetEnumerator();
        for (var i = 0; codesEnum.MoveNext(); i++)
            res[i] = this[(char)codesEnum.Current];

        return res;
    }

    /// <summary>
    ///     Gets the ordered codes.
    /// </summary>
    /// <returns></returns>
    public char[] GetOrderedCodes()
    {
        var keys = new char[Keys.Count];
        Keys.CopyTo(keys, 0);
        for (var i = 0; i < keys.Length; i++)
        {
            var ic = keys[i];
            var tmpIndex = i;
            var tmpChar = ic;
            for (var j = i + 1; j < keys.Length; j++)
                if (keys[j] < tmpChar)
                {
                    tmpIndex = j;
                    tmpChar = keys[j];
                }

            if (tmpIndex != i)
            {
                keys[tmpIndex] = keys[i];
                keys[i] = tmpChar;
            }
        }

        return keys;
    }

    /// <summary>
    ///     Writes to.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BufferedBinaryWriter writer)
    {
        var codes = GetOrderedCodes();
        var glyphs = GetOrderedGlyphs(codes);

        ShapeWithStyle.NumFillBits = 0;
        ShapeWithStyle.NumLineBits = 0;

        var glyphsEnum = glyphs.GetEnumerator();
        while (glyphsEnum.MoveNext())
        {
            ShapeWithStyle.NumFillBits = 1;
            ((ShapeRecordCollection)glyphsEnum.Current).WriteTo(writer);
        }

        var chars = codes.GetEnumerator();
        while (chars.MoveNext())
        {
            var c = (char)chars.Current;
            if (IsWideCodes)
                writer.Write((ushort)c);
            else
                writer.Write((byte)c);
        }
    }

    /// <summary>
    ///     Gets the char index.
    /// </summary>
    /// <param name="character">Character.</param>
    /// <returns>char index if found, -1 else</returns>
    public int GetCharIndex(char character)
    {
        var codes = GetOrderedCodes();
        var chars = codes.GetEnumerator();
        for (var i = 0; chars.MoveNext(); i++)
        {
            var c = (char)chars.Current;
            if (c == character)
                return i;
        }

        return -1;
    }

    /// <summary>
    ///     Serializes to the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        //Write glyphs
        var codes = GetOrderedCodes();
        var glyphs = GetOrderedGlyphs(codes);

        writer.WriteStartElement("GlyphShapesTable");
        var glyphsEnum = glyphs.GetEnumerator();
        while (glyphsEnum.MoveNext())
        {
            // To set the first fillStyle0 to 1.
            ShapeWithStyle.NumFillBits = 1;
            ((ShapeRecordCollection)glyphsEnum.Current).Serialize(writer);
        }

        writer.WriteEndElement();

        //Write codes
        writer.WriteStartElement("CodeTable");
        var chars = codes.GetEnumerator();
        while (chars.MoveNext())
        {
            var code = (char)chars.Current;
            writer.WriteStartElement("Code");
            writer.WriteAttributeString("Value", ((ushort)code).ToString());
            writer.WriteAttributeString("Char", code.ToString());
            writer.WriteEndElement();
        }

        writer.WriteEndElement();
    }

    #endregion
}