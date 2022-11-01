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
using System.IO;
using System.Xml;
using SWF.NET.Utils;

namespace SWF.NET.Tags.Types;

/// <summary>
///     LineStyleCollection class
/// </summary>
public class LineStyleCollection : CollectionBase, ISwfSerializer
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="LineStyleCollection" /> instance.
    /// </summary>
    public LineStyleCollection()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="LineStyleCollection" /> instance.
    /// </summary>
    /// <param name="lineStyles">Line styles.</param>
    public LineStyleCollection(LineStyle[] lineStyles)
    {
        if (lineStyles != null)
            AddRange(lineStyles);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    /// <param name="shapeType">Shape type.</param>
    public void ReadData(BufferedBinaryReader binaryReader, ShapeType shapeType)
    {
        var count = 0;
        var lineStyleCount = binaryReader.ReadByte();
        count = Convert.ToInt32(lineStyleCount);

        ushort lineStyleCountExtended = 0;
        if (lineStyleCount == 0xFF)
        {
            lineStyleCountExtended = binaryReader.ReadUInt16();
            count = Convert.ToInt32(lineStyleCountExtended);
        }

        if (count > 0)
            for (var i = 0; i < count; i++)
            {
                var lineStyle = new LineStyle();
                lineStyle.ReadData(binaryReader, shapeType);
                Add(lineStyle);
            }
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>size of the structure</returns>
    public int GetSizeOf()
    {
        var res = 1;
        var count = Count;
        if (count >= 0xFF)
            res += 2;

        var lineStyles = GetEnumerator();
        while (lineStyles.MoveNext())
            res += ((LineStyle)lineStyles.Current).GetSizeOf();

        return res;
    }

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BinaryWriter writer)
    {
        var count = Count;
        if (count < 0xFF)
            writer.Write((byte)count);
        else
        {
            writer.Write((byte)0xFF);
            writer.Write((ushort)count);
        }

        var lineStyles = GetEnumerator();
        while (lineStyles.MoveNext())
            ((LineStyle)lineStyles.Current).WriteTo(writer);
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("LineStylesArray");

        var lineStyles = GetEnumerator();
        while (lineStyles.MoveNext())
            ((LineStyle)lineStyles.Current).Serialize(writer);

        writer.WriteEndElement();
    }

    #endregion

    #region Collection methods

    /// <summary>
    ///     Adds the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public LineStyle Add(LineStyle value)
    {
        List.Add(value);
        return value;
    }

    /// <summary>
    ///     Adds the range.
    /// </summary>
    /// <param name="values">Values.</param>
    public void AddRange(LineStyle[] values)
    {
        foreach (var ip in values)
            Add(ip);
    }

    /// <summary>
    ///     Removes the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    public void Remove(LineStyle value)
    {
        if (List.Contains(value))
            List.Remove(value);
    }

    /// <summary>
    ///     Inserts the specified index.
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="value">Value.</param>
    public void Insert(int index, LineStyle value) =>
        List.Insert(index, value);

    /// <summary>
    ///     Containses the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public bool Contains(LineStyle value) =>
        List.Contains(value);

    /// <summary>
    ///     Gets or sets the <see cref="LineStyle" /> at the specified index.
    /// </summary>
    /// <value></value>
    public LineStyle this[int index]
    {
        get => (LineStyle)List[index];
        set => List[index] = value;
    }

    /// <summary>
    ///     Get the index of.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public int IndexOf(LineStyle value) =>
        List.IndexOf(value);

    #endregion
}

/// <summary>
///     LineStyle class
/// </summary>
public class LineStyle : SizeStruct, ISwfSerializer
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="LineStyle" /> instance.
    /// </summary>
    public LineStyle()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="LineStyle" /> instance.
    /// </summary>
    /// <param name="width">Width.</param>
    /// <param name="rgb">Rgb.</param>
    public LineStyle(ushort width, RGBColor rgb)
    {
        this.Width = width;
        this.Rgb = rgb;
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    /// <param name="shapeType">Shape type.</param>
    public void ReadData(BufferedBinaryReader binaryReader, ShapeType shapeType)
    {
        SetStartPoint(binaryReader);
        Width = binaryReader.ReadUInt16();
        Rgb = null;
        if (shapeType == ShapeType.Shape3)
        {
            Rgb = new RGBA();
            Rgb.ReadData(binaryReader);
        }
        else if (shapeType == ShapeType.Shape || shapeType == ShapeType.Shape2)
        {
            Rgb = new RGB();
            Rgb.ReadData(binaryReader);
        }

        SetEndPoint(binaryReader);
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>size of this object</returns>
    public int GetSizeOf() =>
        2 + Rgb.GetSizeOf();

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(Width);
        if (Rgb != null)
            Rgb.WriteTo(writer);
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("LineStyle");
        SerializeBinarySize(writer);
        writer.WriteElementString("Width", Width.ToString());
        Rgb.Serialize(writer);
        writer.WriteEndElement();
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the width.
    /// </summary>
    /// <value></value>
    public ushort Width { get; set; }

    /// <summary>
    ///     Gets or sets the RGB.
    /// </summary>
    /// <value></value>
    public RGBColor Rgb { get; set; }

    #endregion
}