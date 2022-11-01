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
///     KerningRecord
/// </summary>
public class KerningRecord : ISwfSerializer
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="KerningRecord" /> instance.
    /// </summary>
    public KerningRecord()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="KerningRecord" /> instance.
    /// </summary>
    /// <param name="fontFlagsWideCodes">Font flags wide codes.</param>
    /// <param name="fontFlagsWideCode1">Font flags wide code1.</param>
    /// <param name="fontFlagsWideCode2">Font flags wide code2.</param>
    /// <param name="fontKerningAdjustement">Font kerning adjustement.</param>
    public KerningRecord(
        bool fontFlagsWideCodes,
        ushort fontFlagsWideCode1,
        ushort fontFlagsWideCode2,
        short fontKerningAdjustement)
    {
        this.FontFlagsWideCodes = fontFlagsWideCodes;
        this.FontFlagsWideCode1 = fontFlagsWideCode1;
        this.FontFlagsWideCode2 = fontFlagsWideCode2;
        this.FontKerningAdjustement = fontKerningAdjustement;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets a value indicating whether [font flags wide codes].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [font flags wide codes]; otherwise, <c>false</c>.
    /// </value>
    public bool FontFlagsWideCodes { get; set; }

    /// <summary>
    ///     Gets or sets the font flags wide code1.
    /// </summary>
    public uint FontFlagsWideCode1 { get; set; }

    /// <summary>
    ///     Gets or sets the font flags wide code2.
    /// </summary>
    public uint FontFlagsWideCode2 { get; set; }

    /// <summary>
    ///     Gets or sets the font kerning adjustement.
    /// </summary>
    public short FontKerningAdjustement { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    /// <param name="fontFlagsWideCodes">Font flags wide codes.</param>
    public void ReadData(BufferedBinaryReader binaryReader, bool fontFlagsWideCodes)
    {
        this.FontFlagsWideCodes = fontFlagsWideCodes;
        if (fontFlagsWideCodes)
        {
            FontFlagsWideCode1 = binaryReader.ReadUInt16();
            FontFlagsWideCode2 = binaryReader.ReadUInt16();
        }
        else
        {
            FontFlagsWideCode1 = binaryReader.ReadByte();
            FontFlagsWideCode2 = binaryReader.ReadByte();
        }

        var fontKerningAdjustement = binaryReader.ReadInt16();
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>Size of this object</returns>
    public int GetSizeOf()
    {
        var res = 2;
        if (FontFlagsWideCodes)
            res += 4;
        else
            res += 2;
        return res;
    }

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BinaryWriter writer)
    {
        if (FontFlagsWideCodes)
        {
            writer.Write((ushort)FontFlagsWideCode1);
            writer.Write((ushort)FontFlagsWideCode2);
        }
        else
        {
            writer.Write((byte)FontFlagsWideCode1);
            writer.Write((byte)FontFlagsWideCode2);
        }

        writer.Write(FontKerningAdjustement);
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("KerningRecord");
        writer.WriteElementString("FontFlagsWideCodes", FontFlagsWideCodes.ToString());
        writer.WriteElementString("FontFlagsWideCode1", FontFlagsWideCode1.ToString());
        writer.WriteElementString("FontFlagsWideCode2", FontFlagsWideCode2.ToString());
        writer.WriteElementString("FontKerningAdjustement", FontKerningAdjustement.ToString());
        writer.WriteEndElement();
    }

    #endregion
}

/// <summary>
///     KerningRecordCollection class
/// </summary>
public class KerningRecordCollection : CollectionBase
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="KerningRecordCollection" /> instance.
    /// </summary>
    public KerningRecordCollection()
    {
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    /// <param name="fontFlagsWideCodes">Font flags wide codes.</param>
    public void ReadData(BufferedBinaryReader binaryReader, bool fontFlagsWideCodes)
    {
        var kerningCount = binaryReader.ReadUInt16();
        if (kerningCount > 0)
            for (var i = 0; i < kerningCount; i++)
            {
                var fontKerning = new KerningRecord();
                fontKerning.ReadData(binaryReader, fontFlagsWideCodes);
                Add(fontKerning);
            }
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf()
    {
        var res = 2;
        var kernings = GetEnumerator();
        while (kernings.MoveNext())
            res += ((KerningRecord)kernings.Current).GetSizeOf();
        return res;
    }

    /// <summary>
    ///     Writes to.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BufferedBinaryWriter writer)
    {
        writer.Write((ushort)Count);
        var kernings = GetEnumerator();
        while (kernings.MoveNext())
            ((KerningRecord)kernings.Current).WriteTo(writer);
    }

    /// <summary>
    ///     Serializes to the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("FontKerningTable");
        var kernings = GetEnumerator();
        while (kernings.MoveNext())
            ((KerningRecord)kernings.Current).Serialize(writer);
        writer.WriteEndElement();
    }

    #endregion

    #region Collection methods

    /// <summary>
    ///     Adds the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public KerningRecord Add(KerningRecord value)
    {
        List.Add(value);
        return value;
    }

    /// <summary>
    ///     Adds the range.
    /// </summary>
    /// <param name="values">Values.</param>
    public void AddRange(KerningRecord[] values)
    {
        foreach (var ip in values)
            Add(ip);
    }

    /// <summary>
    ///     Removes the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    public void Remove(KerningRecord value)
    {
        if (List.Contains(value))
            List.Remove(value);
    }

    /// <summary>
    ///     Inserts the specified index.
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="value">Value.</param>
    public void Insert(int index, KerningRecord value) =>
        List.Insert(index, value);

    /// <summary>
    ///     Containses the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public bool Contains(KerningRecord value) =>
        List.Contains(value);

    /// <summary>
    ///     Gets or sets the <see cref="KerningRecord" /> at the specified index.
    /// </summary>
    /// <value></value>
    public KerningRecord this[int index]
    {
        get => (KerningRecord)List[index];
        set => List[index] = value;
    }

    /// <summary>
    ///     Get the index of.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public int IndexOf(KerningRecord value) =>
        List.IndexOf(value);

    #endregion
}