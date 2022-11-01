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

using System.Xml;
using SWF.NET.Utils;

namespace SWF.NET.Tags.Types;

/// <summary>
///     ShapeWithStyle class
/// </summary>
public class ShapeWithStyle : SizeStruct, ISwfSerializer
{
    #region Members

    /// <summary>
    ///     Current number of fill bits
    /// </summary>
    public static uint NumFillBits;

    /// <summary>
    ///     Current number of line bits
    /// </summary>
    public static uint NumLineBits;

    #endregion

    #region Ctor & Init

    /// <summary>
    ///     Creates a new <see cref="ShapeWithStyle" /> instance.
    /// </summary>
    public ShapeWithStyle() =>
        Init();

    /// <summary>
    ///     Creates a new <see cref="ShapeWithStyle" /> instance.
    /// </summary>
    /// <param name="fillStyleArray">Fill style array.</param>
    /// <param name="lineStyleArray">Line style array.</param>
    /// <param name="shapes">Shapes.</param>
    public ShapeWithStyle(
        FillStyleCollection fillStyleArray,
        LineStyleCollection lineStyleArray,
        ShapeRecordCollection shapes)
    {
        FillStyleArray = fillStyleArray;
        LineStyleArray = lineStyleArray;
        Shapes = shapes;
    }

    /// <summary>
    ///     Inits this instance.
    /// </summary>
    private void Init()
    {
        FillStyleArray = new FillStyleCollection();
        LineStyleArray = new LineStyleCollection();
        Shapes = new ShapeRecordCollection();
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets the fill style array.
    /// </summary>
    /// <value></value>
    public FillStyleCollection FillStyleArray { get; private set; }

    /// <summary>
    ///     Gets the line style array.
    /// </summary>
    /// <value></value>
    public LineStyleCollection LineStyleArray { get; private set; }

    /// <summary>
    ///     Gets the shapes.
    /// </summary>
    /// <value></value>
    public ShapeRecordCollection Shapes { get; private set; }

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

        FillStyleArray = new FillStyleCollection();
        FillStyleArray.ReadData(binaryReader, shapeType);

        LineStyleArray = new LineStyleCollection();
        LineStyleArray.ReadData(binaryReader, shapeType);

        Shapes = new ShapeRecordCollection();
        Shapes.ReadData(binaryReader, shapeType);

        SetEndPoint(binaryReader);
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>Size of this object.</returns>
    public int GetSizeOf()
    {
        var res = 0;
        if (FillStyleArray != null)
            res += FillStyleArray.GetSizeOf();
        if (LineStyleArray != null)
            res += LineStyleArray.GetSizeOf();
        if (Shapes != null)
        {
            NumFillBits = 0;
            NumLineBits = 0;
            if (FillStyleArray != null && FillStyleArray.Count != 0)
                NumFillBits = BufferedBinaryWriter.GetNumBits((uint)FillStyleArray.Count);
            if (LineStyleArray != null && LineStyleArray.Count != 0)
                NumLineBits = BufferedBinaryWriter.GetNumBits((uint)LineStyleArray.Count);
            res += Shapes.GetSizeOf();
            NumFillBits = 0;
            NumLineBits = 0;
        }

        return res;
    }

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BufferedBinaryWriter writer)
    {
        if (FillStyleArray != null)
            FillStyleArray.WriteTo(writer);
        if (LineStyleArray != null)
            LineStyleArray.WriteTo(writer);
        if (Shapes != null)
        {
            NumFillBits = 0;
            NumLineBits = 0;
            if (FillStyleArray != null && FillStyleArray.Count != 0)
                NumFillBits = BufferedBinaryWriter.GetNumBits((uint)FillStyleArray.Count);
            if (LineStyleArray != null && LineStyleArray.Count != 0)
                NumLineBits = BufferedBinaryWriter.GetNumBits((uint)LineStyleArray.Count);
            Shapes.WriteTo(writer);
            NumFillBits = 0;
            NumLineBits = 0;
        }
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("ShapeWithStyle");
        SerializeBinarySize(writer);

        if (FillStyleArray != null)
            FillStyleArray.Serialize(writer);
        if (LineStyleArray != null)
            LineStyleArray.Serialize(writer);
        if (Shapes != null)
        {
            NumFillBits = 0;
            NumLineBits = 0;
            if (FillStyleArray != null && FillStyleArray.Count != 0)
                NumFillBits = BufferedBinaryWriter.GetNumBits((uint)FillStyleArray.Count);
            if (LineStyleArray != null && LineStyleArray.Count != 0)
                NumLineBits = BufferedBinaryWriter.GetNumBits((uint)LineStyleArray.Count);

            Shapes.Serialize(writer);

            NumFillBits = 0;
            NumLineBits = 0;
        }

        writer.WriteEndElement();
    }

    #endregion
}