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
///     SoundInfo
/// </summary>
public class SoundInfo : ISwfSerializer
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="SoundInfo" /> instance.
    /// </summary>
    public SoundInfo() =>
        EnvelopeRecord = new SoundEnvelopeCollection();

    #endregion

    #region Members

    #endregion

    #region Properties

    /// <summary>
    ///     Gets a value indicating whether this instance has out point.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance has out point; otherwise, <c>false</c>.
    /// </value>
    private bool HasOutPoint =>
        OutPoint != 0;

    /// <summary>
    ///     Gets a value indicating whether this instance has in point.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance has in point; otherwise, <c>false</c>.
    /// </value>
    private bool HasInPoint =>
        InPoint != 0;

    /// <summary>
    ///     Gets a value indicating whether this instance has loops.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance has loops; otherwise, <c>false</c>.
    /// </value>
    private bool HasLoops =>
        LoopCount != 0;

    /// <summary>
    ///     Gets a value indicating whether this instance has envelope.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance has envelope; otherwise, <c>false</c>.
    /// </value>
    private bool HasEnvelope =>
        EnvelopeRecord != null &&
        EnvelopeRecord.Count != 0;

    /// <summary>
    ///     Gets the envelope record.
    /// </summary>
    /// <value></value>
    public SoundEnvelopeCollection EnvelopeRecord { get; }

    /// <summary>
    ///     Gets or sets the out point.
    /// </summary>
    /// <value></value>
    public uint OutPoint { get; set; }

    /// <summary>
    ///     Gets or sets the in point.
    /// </summary>
    /// <value></value>
    public uint InPoint { get; set; }

    /// <summary>
    ///     Gets or sets the loop count.
    /// </summary>
    /// <value></value>
    public ushort LoopCount { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether [sync no multiple].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [sync no multiple]; otherwise, <c>false</c>.
    /// </value>
    public bool SyncNoMultiple { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether [sync stop].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [sync stop]; otherwise, <c>false</c>.
    /// </value>
    public bool SyncStop { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>Size of this object.</returns>
    public int GetSizeOf()
    {
        var res = 1;
        if (HasInPoint)
            res += 4;
        if (HasOutPoint)
            res += 4;
        if (HasLoops)
            res += 2;
        if (HasEnvelope)
        {
            res++;
            if (EnvelopeRecord != null)
                res += EnvelopeRecord.Count * SoundEnvelope.GetSizeOf();
        }

        return res;
    }

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public void ReadData(BufferedBinaryReader binaryReader)
    {
        binaryReader.ReadUBits(2);

        SyncStop = binaryReader.ReadBoolean();
        SyncNoMultiple = binaryReader.ReadBoolean();
        var hasEnvelope = binaryReader.ReadBoolean();
        var hasLoops = binaryReader.ReadBoolean();
        var hasOutPoint = binaryReader.ReadBoolean();
        var hasInPoint = binaryReader.ReadBoolean();

        if (hasInPoint)
            InPoint = binaryReader.ReadUInt32();
        if (hasOutPoint)
            OutPoint = binaryReader.ReadUInt32();
        if (hasLoops)
            LoopCount = binaryReader.ReadUInt16();
        if (hasEnvelope)
        {
            var envPoints = binaryReader.ReadByte();
            if (envPoints != 0)
            {
                EnvelopeRecord.Clear();
                for (var i = 0; i < envPoints; i++)
                {
                    var envelope = new SoundEnvelope();
                    envelope.ReadData(binaryReader);
                    EnvelopeRecord.Add(envelope);
                }
            }
        }
    }

    /// <summary>
    ///     Writes to binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BufferedBinaryWriter writer)
    {
        writer.WriteUBits(0, 2);
        writer.WriteBoolean(SyncStop);
        writer.WriteBoolean(SyncNoMultiple);
        writer.WriteBoolean(HasEnvelope);
        writer.WriteBoolean(HasLoops);
        writer.WriteBoolean(HasOutPoint);
        writer.WriteBoolean(HasInPoint);

        if (HasInPoint)
            writer.Write(InPoint);
        if (HasOutPoint)
            writer.Write(OutPoint);
        if (HasLoops)
            writer.Write(LoopCount);

        if (HasEnvelope)
        {
            writer.Write((byte)EnvelopeRecord.Count);
            var envelopes = EnvelopeRecord.GetEnumerator();
            while (envelopes.MoveNext())
                ((SoundEnvelope)envelopes.Current).WriteTo(writer);
        }
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("SoundInfo");
        writer.WriteElementString("SyncStop", SyncStop.ToString());
        writer.WriteElementString("SyncNoMultiple", SyncNoMultiple.ToString());
        writer.WriteElementString("HasEnvelope", HasEnvelope.ToString());
        writer.WriteElementString("HasLoops", HasLoops.ToString());
        writer.WriteElementString("HasOutPoint", HasOutPoint.ToString());
        writer.WriteElementString("HasInPoint", HasInPoint.ToString());

        if (HasInPoint)
            writer.WriteElementString("InPoint", InPoint.ToString());
        if (HasOutPoint)
            writer.WriteElementString("OutPoint", OutPoint.ToString());
        if (HasLoops)
            writer.WriteElementString("LoopCount", LoopCount.ToString());

        if (HasEnvelope)
        {
            var envelopes = EnvelopeRecord.GetEnumerator();
            while (envelopes.MoveNext())
                ((SoundEnvelope)envelopes.Current).Serialize(writer);
        }

        writer.WriteEndElement();
    }

    #endregion
}

/// <summary>
///     SoundEnvelope
/// </summary>
public class SoundEnvelope : ISwfSerializer
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="SoundEnvelope" /> instance.
    /// </summary>
    public SoundEnvelope()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="SoundEnvelope" /> instance.
    /// </summary>
    /// <param name="pos44">Pos44.</param>
    /// <param name="leftLevel">Left level.</param>
    /// <param name="rightLevel">Right level.</param>
    public SoundEnvelope(uint pos44, ushort leftLevel, ushort rightLevel)
    {
        Pos44 = pos44;
        LeftLevel = leftLevel;
        RightLevel = rightLevel;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the right level.
    /// </summary>
    /// <value></value>
    public ushort RightLevel { get; set; }

    /// <summary>
    ///     Gets or sets the left level.
    /// </summary>
    /// <value></value>
    public ushort LeftLevel { get; set; }

    /// <summary>
    ///     Gets or sets the pos44.
    /// </summary>
    /// <value></value>
    public uint Pos44 { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public void ReadData(BufferedBinaryReader binaryReader)
    {
        Pos44 = binaryReader.ReadUInt32();
        LeftLevel = binaryReader.ReadUInt16();
        RightLevel = binaryReader.ReadUInt16();
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>size of this object</returns>
    public static int GetSizeOf() =>
        4 + 2 + 2;

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(Pos44);
        writer.Write(LeftLevel);
        writer.Write(RightLevel);
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("SoundEnvelope");
        writer.WriteElementString("Pos44", Pos44.ToString());
        writer.WriteElementString("LeftLevel", LeftLevel.ToString());
        writer.WriteElementString("RightLevel", RightLevel.ToString());
        writer.WriteEndElement();
    }

    #endregion
}

/// <summary>
///     SoundEnvelopeCollection class
/// </summary>
public class SoundEnvelopeCollection : CollectionBase, ISwfSerializer
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="SoundEnvelopeCollection" /> instance.
    /// </summary>
    public SoundEnvelopeCollection()
    {
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("SoundEnvelopeCollection");

        var envs = GetEnumerator();
        while (envs.MoveNext())
            ((SoundEnvelope)envs.Current).Serialize(writer);

        writer.WriteEndElement();
    }

    #endregion

    #region Collection methods

    /// <summary>
    ///     Adds the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public SoundEnvelope Add(SoundEnvelope value)
    {
        List.Add(value);
        return value;
    }

    /// <summary>
    ///     Adds the range.
    /// </summary>
    /// <param name="values">Values.</param>
    public void AddRange(SoundEnvelope[] values)
    {
        var val = values.GetEnumerator();
        while (val.MoveNext())
            Add((SoundEnvelope)val.Current);
    }

    /// <summary>
    ///     Removes the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    public void Remove(SoundEnvelope value)
    {
        if (List.Contains(value))
            List.Remove(value);
    }

    /// <summary>
    ///     Inserts the specified index.
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="value">Value.</param>
    public void Insert(int index, SoundEnvelope value) =>
        List.Insert(index, value);

    /// <summary>
    ///     Containses the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public bool Contains(SoundEnvelope value) =>
        List.Contains(value);

    /// <summary>
    ///     Gets or sets the <see cref="SoundEnvelope" /> at the specified index.
    /// </summary>
    /// <value></value>
    public SoundEnvelope this[int index]
    {
        get => (SoundEnvelope)List[index];
        set => List[index] = value;
    }

    /// <summary>
    ///     Get the index of.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public int IndexOf(SoundEnvelope value) =>
        List.IndexOf(value);

    #endregion
}