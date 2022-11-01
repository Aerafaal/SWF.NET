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
using System.IO;
using System.Xml;
using SWF.NET.Utils;

namespace SWF.NET.Tags.Types;

/// <summary>
///     MorphGradRecord
/// </summary>
public class MorphGradRecord : ISwfSerializer
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="MorphGradRecord" /> instance.
    /// </summary>
    public MorphGradRecord()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="MorphGradRecord" /> instance.
    /// </summary>
    /// <param name="startRatio">Start ratio.</param>
    /// <param name="startColor">Color of the start.</param>
    /// <param name="endRatio">End ratio.</param>
    /// <param name="endColor">Color of the end.</param>
    public MorphGradRecord(byte startRatio, RGBA startColor, byte endRatio, RGBA endColor)
    {
        this.StartRatio = startRatio;
        this.StartColor = startColor;
        this.EndRatio = endRatio;
        this.EndColor = endColor;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the start ratio.
    /// </summary>
    public byte StartRatio { get; set; }

    /// <summary>
    ///     Gets or sets the color of the start.
    /// </summary>
    public RGBA StartColor { get; set; }

    /// <summary>
    ///     Gets or sets the end ratio.
    /// </summary>
    public byte EndRatio { get; set; }

    /// <summary>
    ///     Gets or sets the color of the end.
    /// </summary>
    public RGBA EndColor { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public void ReadData(BufferedBinaryReader binaryReader)
    {
        StartRatio = binaryReader.ReadByte();
        StartColor = new RGBA();
        StartColor.ReadData(binaryReader);
        EndRatio = binaryReader.ReadByte();
        EndColor = new RGBA();
        EndColor.ReadData(binaryReader);
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>size of this structure</returns>
    public static int GetSizeOf() =>
        2 + 2 * 4;

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(StartRatio);
        StartColor.WriteTo(writer);
        writer.Write(EndRatio);
        EndColor.WriteTo(writer);
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("MorphGradRecord");
        StartColor.Serialize(writer);
        EndColor.Serialize(writer);
        writer.WriteEndElement();
    }

    #endregion
}

/// <summary>
///     MorphGradientCollection
/// </summary>
public class MorphGradientCollection : CollectionBase, ISwfSerializer
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="MorphGradientCollection" /> instance.
    /// </summary>
    public MorphGradientCollection()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="MorphGradientCollection" /> instance.
    /// </summary>
    /// <param name="graphRecords">Graph records.</param>
    public MorphGradientCollection(MorphGradRecord[] graphRecords)
    {
        if (graphRecords != null)
            AddRange(graphRecords);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public void ReadData(BufferedBinaryReader binaryReader)
    {
        var numGradients = binaryReader.ReadByte();

        for (var i = 0; i < numGradients; i++)
        {
            var morph = new MorphGradRecord();
            morph.ReadData(binaryReader);
            Add(morph);
        }
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>size of this object</returns>
    public int GetSizeOf()
    {
        var res = 1;
        res += Count * MorphGradRecord.GetSizeOf();
        return res;
    }

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write((byte)Count);
        foreach (MorphGradRecord mgr in this)
            mgr.WriteTo(writer);
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("MorphGradientCollection");
        foreach (MorphGradRecord mgr in this)
            mgr.Serialize(writer);
        writer.WriteEndElement();
    }

    #endregion

    #region Collection methods

    /// <summary>
    ///     Adds the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public MorphGradRecord Add(MorphGradRecord value)
    {
        List.Add(value);
        return value;
    }

    /// <summary>
    ///     Adds the range.
    /// </summary>
    /// <param name="values">Values.</param>
    public void AddRange(MorphGradRecord[] values)
    {
        foreach (var ip in values)
            Add(ip);
    }

    /// <summary>
    ///     Removes the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    public void Remove(MorphGradRecord value)
    {
        if (List.Contains(value))
            List.Remove(value);
    }

    /// <summary>
    ///     Inserts the specified index.
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="value">Value.</param>
    public void Insert(int index, MorphGradRecord value) =>
        List.Insert(index, value);

    /// <summary>
    ///     Containses the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public bool Contains(MorphGradRecord value) =>
        List.Contains(value);

    /// <summary>
    ///     Gets or sets the <see cref="LineStyle" /> at the specified index.
    /// </summary>
    /// <value></value>
    public MorphGradRecord this[int index]
    {
        get => (MorphGradRecord)List[index];
        set => List[index] = value;
    }

    /// <summary>
    ///     Get the index of.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public int IndexOf(MorphGradRecord value) =>
        List.IndexOf(value);

    #endregion
}