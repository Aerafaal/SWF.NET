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
using log4net;
using SWF.NET.Utils;

namespace SWF.NET.Tags.Types;

/// <summary>
///     GlyphShapesCollection
/// </summary>
public class GlyphShapesCollection : CollectionBase, ISwfSerializer
{
    #region Members

    private static readonly ILog log = LogManager.GetLogger(typeof(ShapeRecordCollection));

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="GlyphShapesCollection" /> instance.
    /// </summary>
    public GlyphShapesCollection()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="ShapeRecordCollection" /> instance.
    /// </summary>
    /// <param name="shapes">Shapes.</param>
    public GlyphShapesCollection(ShapeRecordCollection[] shapes)
    {
        if (shapes != null)
            AddRange(shapes);
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

        var shapes = GetEnumerator();
        while (shapes.MoveNext())
            res += ((ShapeRecordCollection)shapes.Current).GetSizeOf();
        return res;
    }

    /// <summary>
    ///     Writes to.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BufferedBinaryWriter writer)
    {
        var shapes = GetEnumerator();
        while (shapes.MoveNext())
            ((ShapeRecordCollection)shapes.Current).WriteTo(writer);
    }

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="numGlyphs">Num glyphs.</param>
    public void ReadData(BufferedBinaryReader reader, ushort numGlyphs)
    {
        for (var i = 0; i < numGlyphs; i++)
        {
            var glyphShape = new ShapeRecordCollection();
            glyphShape.ReadData(reader, ShapeType.None);
            Add(glyphShape);
        }
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("GlyphShapesTable");

        var shapes = GetEnumerator();
        while (shapes.MoveNext())
            ((ShapeRecordCollection)shapes.Current).Serialize(writer);

        writer.WriteEndElement();
    }

    #endregion

    #region Collection methods

    /// <summary>
    ///     Gets the last ShapeRecordCollection of the collection.
    /// </summary>
    /// <returns></returns>
    public ShapeRecordCollection GetLastOne()
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
    public ShapeRecordCollection Add(ShapeRecordCollection value)
    {
        List.Add(value);
        return value;
    }

    /// <summary>
    ///     Adds the range.
    /// </summary>
    /// <param name="values">Values.</param>
    public void AddRange(ShapeRecordCollection[] values)
    {
        if (values == null)
            return;

        var enu = values.GetEnumerator();
        while (enu.MoveNext())
            Add((ShapeRecordCollection)enu.Current);
    }

    /// <summary>
    ///     Removes the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    public void Remove(ShapeRecordCollection value)
    {
        if (List.Contains(value))
            List.Remove(value);
    }

    /// <summary>
    ///     Inserts the specified index.
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="value">Value.</param>
    public void Insert(int index, ShapeRecordCollection value) =>
        List.Insert(index, value);

    /// <summary>
    ///     Containses the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public bool Contains(ShapeRecordCollection value) =>
        List.Contains(value);

    /// <summary>
    ///     Gets or sets the <see cref="LineStyle" /> at the specified index.
    /// </summary>
    /// <value></value>
    public ShapeRecordCollection this[int index]
    {
        get => (ShapeRecordCollection)List[index];
        set => List[index] = value;
    }

    /// <summary>
    ///     Get the index of.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public int IndexOf(ShapeRecordCollection value) =>
        List.IndexOf(value);

    #endregion
}