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
using log4net;
using SWF.NET.Utils;

namespace SWF.NET.Tags.Types;

/// <summary>
///     ShapeRecordCollection
/// </summary>
public class ShapeRecordCollection : CollectionBase, ISwfSerializer
{
    #region Members

    private static readonly ILog log = LogManager.GetLogger(typeof(ShapeRecordCollection));

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="ShapeRecordCollection" /> instance.
    /// </summary>
    public ShapeRecordCollection()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="ShapeRecordCollection" /> instance.
    /// </summary>
    /// <param name="shapes">Shapes.</param>
    public ShapeRecordCollection(ShapeRecord[] shapes)
    {
        if (shapes != null)
            AddRange(shapes);
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
        binaryReader.SynchBits();
        var numFillBits = (byte)binaryReader.ReadUBits(4);
        var numLineBits = (byte)binaryReader.ReadUBits(4);

        var readEndShapeRecord = false;
        while (!readEndShapeRecord)
        {
            var type = binaryReader.ReadBoolean();
            var flags = (byte)binaryReader.ReadUBits(5);

            if (type == false)
            {
                //Non-edge record
                if (flags == 0)
                {
                    //EndShapeRecord
                    readEndShapeRecord = true;
                    Add(new EndShapeRecord());
                    if (log.IsInfoEnabled)
                        log.Info("Shape: EndShapeRecord");
                }
                else
                {
                    //StyleChangerecord
                    var styleChange = new StyleChangeRecord();
                    styleChange.ReadData(binaryReader, flags, ref numFillBits, ref numLineBits, shapeType);
                    Add(styleChange);
                    if (log.IsInfoEnabled)
                        log.Info("Shape: StyleChangeRecord");
                }
            }
            else
            {
                //Edge record
                if ((flags & 0x10) != 0)
                {
                    //StraightedEdgeRecord
                    var straight = new StraightEdgeRecord();
                    straight.ReadData(binaryReader, flags);
                    Add(straight);
                    if (log.IsInfoEnabled)
                        log.Info("Shape: StraightedEdgeRecord");
                }
                else
                {
                    //CurvedEdgeRecord
                    var curved = new CurvedEdgeRecord();
                    curved.ReadData(binaryReader, flags);
                    Add(curved);
                    if (log.IsInfoEnabled)
                        log.Info("Shape: CurvedEdgeRecord");
                }
            }
        }
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>Size of this object.</returns>
    public int GetSizeOf()
    {
        var res = 8;

        var shapes = GetEnumerator();
        while (shapes.MoveNext())
            res += ((ShapeRecord)shapes.Current).GetBitSizeOf(res);

        var lastShape = GetLastOne();
        if (lastShape != null && !(lastShape is EndShapeRecord))
        {
            var end = new EndShapeRecord();
            res += end.GetBitSizeOf(res);
        }

        return Convert.ToInt32(Math.Ceiling(res / 8.0));
    }

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BufferedBinaryWriter writer)
    {
        writer.SynchBits();
        writer.WriteUBits(ShapeWithStyle.NumFillBits, 4);
        writer.WriteUBits(ShapeWithStyle.NumLineBits, 4);

        var lastShape = GetLastOne();
        if (lastShape != null && !(lastShape is EndShapeRecord))
            Add(new EndShapeRecord());

        var shapes = GetEnumerator();
        while (shapes.MoveNext())
            ((ShapeRecord)shapes.Current).WriteTo(writer);
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("ShapeCollection");
        writer.WriteAttributeString("NumFillBits", ShapeWithStyle.NumFillBits.ToString());
        writer.WriteAttributeString("NumLineBits", ShapeWithStyle.NumLineBits.ToString());

        var shapes = GetEnumerator();
        while (shapes.MoveNext())
            ((ShapeRecord)shapes.Current).Serialize(writer);
        writer.WriteEndElement();
    }

    #endregion

    #region Collection methods

    /// <summary>
    ///     Gets the last ShapeRecord of the collection.
    /// </summary>
    /// <returns></returns>
    public ShapeRecord GetLastOne()
    {
        if (Count == 0)
            return null;

        return this[Count - 1];
    }

    /// <summary>
    ///     Adds the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public ShapeRecord Add(ShapeRecord value)
    {
        List.Add(value);
        return value;
    }

    /// <summary>
    ///     Adds the range.
    /// </summary>
    /// <param name="values">Values.</param>
    public void AddRange(ShapeRecord[] values)
    {
        foreach (var ip in values)
            Add(ip);
    }

    /// <summary>
    ///     Adds the range.
    /// </summary>
    /// <param name="values">Values.</param>
    public void AddRange(ShapeRecordCollection values)
    {
        if (values == null)
            return;

        var enu = values.GetEnumerator();
        while (enu.MoveNext())
            Add((ShapeRecord)enu.Current);
    }

    /// <summary>
    ///     Removes the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    public void Remove(ShapeRecord value)
    {
        if (List.Contains(value))
            List.Remove(value);
    }

    /// <summary>
    ///     Inserts the specified index.
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="value">Value.</param>
    public void Insert(int index, ShapeRecord value) =>
        List.Insert(index, value);

    /// <summary>
    ///     Containses the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public bool Contains(ShapeRecord value) =>
        List.Contains(value);

    /// <summary>
    ///     Gets or sets the <see cref="LineStyle" /> at the specified index.
    /// </summary>
    /// <value></value>
    public ShapeRecord this[int index]
    {
        get => (ShapeRecord)List[index];
        set => List[index] = value;
    }

    /// <summary>
    ///     Get the index of.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public int IndexOf(ShapeRecord value) =>
        List.IndexOf(value);

    #endregion
}