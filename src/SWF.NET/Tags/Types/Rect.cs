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
using System.Drawing;
using System.Xml;
using SWF.NET.Utils;

namespace SWF.NET.Tags.Types;

/// <summary>
///     Rect
/// </summary>
public class Rect : SizeStruct, ISwfSerializer
{
    #region SwfSerializer Members

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("Rect");
        SerializeBinarySize(writer);
        writer.WriteAttributeString("xMin", XMin.ToString());
        writer.WriteAttributeString("yMin", YMin.ToString());
        writer.WriteAttributeString("xMax", XMax.ToString());
        writer.WriteAttributeString("yMax", YMax.ToString());
        writer.WriteEndElement();
    }

    #endregion

    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="Rect" /> instance.
    /// </summary>
    public Rect()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="Rect" /> instance.
    /// </summary>
    /// <param name="xMin">X min (in twips unit: 1 px = 20 twips).</param>
    /// <param name="yMin">Y min (in twips unit: 1 px = 20 twips).</param>
    /// <param name="xMax">X max (in twips unit: 1 px = 20 twips).</param>
    /// <param name="yMax">Y max (in twips unit: 1 px = 20 twips).</param>
    public Rect(int xMin, int yMin, int xMax, int yMax)
    {
        XMin = xMin;
        XMax = xMax;
        YMin = yMin;
        YMax = yMax;
    }

    /// <summary>
    ///     Creates a new <see cref="Rect" /> instance.
    /// </summary>
    /// <param name="xMax">X max (in twips unit: 1 px = 20 twips).</param>
    /// <param name="yMax">Y max (in twips unit: 1 px = 20 twips).</param>
    public Rect(int xMax, int yMax)
    {
        XMin = 0;
        XMax = xMax;
        YMin = 0;
        YMax = yMax;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the pixel rectangle.
    /// </summary>
    public Rectangle Rectangle
    {
        get
        {
            var xMinPix = XMin / 20;
            var yMinPix = YMin / 20;
            var xMaxPix = XMax / 20;
            var yMaxPix = YMax / 20;
            return Rectangle.FromLTRB(xMinPix, yMinPix, xMaxPix, yMaxPix);
        }
        set
        {
            XMin = value.Left * 20;
            YMin = value.Top * 20;
            XMax = value.Right * 20;
            YMax = value.Bottom * 20;
        }
    }

    /// <summary>
    ///     Gets or sets the X min in twips unit.
    /// </summary>
    public int XMin { get; set; }

    /// <summary>
    ///     Gets or sets the X max in twips unit.
    /// </summary>
    public int XMax { get; set; }

    /// <summary>
    ///     Gets or sets the Y min in twips unit.
    /// </summary>
    public int YMin { get; set; }

    /// <summary>
    ///     Gets or sets the Y max in twips unit.
    /// </summary>
    public int YMax { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data from a binary file
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public void ReadData(BufferedBinaryReader binaryReader)
    {
        SetStartPoint(binaryReader);

        var nBits = binaryReader.ReadUBits(5);
        XMin = binaryReader.ReadSBits(nBits);
        XMax = binaryReader.ReadSBits(nBits);
        YMin = binaryReader.ReadSBits(nBits);
        YMax = binaryReader.ReadSBits(nBits);

        SetEndPoint(binaryReader);
    }

    /// <summary>
    ///     Gets the num bits.
    /// </summary>
    /// <returns></returns>
    private uint GetNumBits()
    {
        uint res = 0;
        var num = BufferedBinaryWriter.GetNumBits(XMin);
        if (num > res)
            res = num;
        num = BufferedBinaryWriter.GetNumBits(XMax);
        if (num > res)
            res = num;
        num = BufferedBinaryWriter.GetNumBits(YMin);
        if (num > res)
            res = num;
        num = BufferedBinaryWriter.GetNumBits(YMax);
        if (num > res)
            res = num;
        return res;
    }

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BufferedBinaryWriter writer)
    {
        var nBits = GetNumBits();
        writer.WriteUBits(nBits, 5);
        writer.WriteSBits(XMin, nBits);
        writer.WriteSBits(XMax, nBits);
        writer.WriteSBits(YMin, nBits);
        writer.WriteSBits(YMax, nBits);
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>Size of this object</returns>
    public int GetSizeOf()
    {
        var res = 5;
        var num = GetNumBits();
        res += (int)num * 4;
        res = Convert.ToInt32(Math.Ceiling(res / 8.0));
        return res;
    }

    #endregion
}

/// <summary>
///     RectCollection class
/// </summary>
public class RectCollection : CollectionBase
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="RectCollection" /> instance.
    /// </summary>
    public RectCollection()
    {
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    /// <param name="numGlyphs">Num glyphs.</param>
    public void ReadData(BufferedBinaryReader binaryReader, ushort numGlyphs)
    {
        Clear();
        for (var i = 0; i < numGlyphs; i++)
        {
            binaryReader.SynchBits();
            var fontBound = new Rect();
            fontBound.ReadData(binaryReader);
            Add(fontBound);
        }
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf()
    {
        var res = 0;
        var rects = GetEnumerator();
        while (rects.MoveNext())
            res += ((Rect)rects.Current).GetSizeOf();
        return res;
    }

    /// <summary>
    ///     Writes to.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BufferedBinaryWriter writer)
    {
        var rects = GetEnumerator();
        while (rects.MoveNext())
        {
            writer.SynchBits();
            ((Rect)rects.Current).WriteTo(writer);
        }
    }

    /// <summary>
    ///     Serializes to the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("BoundsTable");
        var rects = GetEnumerator();
        while (rects.MoveNext())
            ((Rect)rects.Current).Serialize(writer);
        writer.WriteEndElement();
    }

    #endregion

    #region Collection methods

    /// <summary>
    ///     Adds the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public Rect Add(Rect value)
    {
        List.Add(value);
        return value;
    }

    /// <summary>
    ///     Adds the range.
    /// </summary>
    /// <param name="values">Values.</param>
    public void AddRange(Rect[] values)
    {
        foreach (var ip in values)
            Add(ip);
    }

    /// <summary>
    ///     Removes the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    public void Remove(Rect value)
    {
        if (List.Contains(value))
            List.Remove(value);
    }

    /// <summary>
    ///     Inserts the specified index.
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="value">Value.</param>
    public void Insert(int index, Rect value) =>
        List.Insert(index, value);

    /// <summary>
    ///     Containses the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public bool Contains(Rect value) =>
        List.Contains(value);

    /// <summary>
    ///     Gets or sets the <see cref="Rect" /> at the specified index.
    /// </summary>
    /// <value></value>
    public Rect this[int index]
    {
        get => (Rect)List[index];
        set => List[index] = value;
    }

    /// <summary>
    ///     Get the index of.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public int IndexOf(Rect value) =>
        List.IndexOf(value);

    #endregion
}