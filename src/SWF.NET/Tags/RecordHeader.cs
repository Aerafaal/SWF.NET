/*
	SwfDotNet is an open source library for writing and reading 
	Macromedia Flash (SWF) bytecode.
	Copyright (C) 2005 Olivier Carpentier - Adelina foundation
	see Licence.cs for GPL full text!
		
	SwfDotNet.IO uses a part of the open source library SwfOp actionscript 
	byte code management, writted by Florian Krüsch, Copyright (C) 2004 .
	
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
using SWF.NET.Exceptions;
using SWF.NET.Utils;

namespace SWF.NET.Tags;

/// <summary>
///     RecordHeader object represents swf tag headers.
/// </summary>
/// <remarks>
///     <p>
///         The Swf format is composed of a series of tagged data blocks.
///         All tag begins with record header informations.
///     </p>
///     <p>
///         A record header object contains the current tag code and
///         the size of the tag in bytes.
///     </p>
///     <p>
///         You don't need to instance this object, to construct a tag.
///         Record header management is automatically done on the reading
///         or the writing process of a tag.
///     </p>
/// </remarks>
internal class RecordHeader
{
    #region Members

    private ushort tagCode;
    private readonly bool longTag;

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="RecordHeader" /> instance.
    /// </summary>
    public RecordHeader()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="RecordHeader" /> instance.
    /// </summary>
    /// <param name="tag">Tag code.</param>
    /// <param name="length">Length.</param>
    public RecordHeader(int tag, int length)
    {
        tagCode = Convert.ToUInt16(tag);
        TagLength = Convert.ToUInt32(length);
        if (TagLength > 0x3e)
            longTag = true;
        else
            longTag = false;
    }

    /// <summary>
    ///     Creates a new <see cref="RecordHeader" /> instance.
    /// </summary>
    /// <param name="longTag">Long tag.</param>
    public RecordHeader(bool longTag) =>
        this.longTag = longTag;

    /// <summary>
    ///     Creates a new <see cref="RecordHeader" /> instance.
    /// </summary>
    /// <param name="tag">Tag code.</param>
    /// <param name="length">Length.</param>
    /// <param name="longTag">Long tag encoding.</param>
    public RecordHeader(int tag, int length, bool longTag)
    {
        tagCode = Convert.ToUInt16(tag);
        TagLength = Convert.ToUInt32(length);
        this.longTag = longTag;
    }

    /// <summary>
    ///     Creates a new <see cref="RecordHeader" /> instance.
    /// </summary>
    /// <param name="tag">Tag code.</param>
    /// <param name="length">Length.</param>
    public RecordHeader(int tag, uint length)
    {
        tagCode = Convert.ToUInt16(tag);
        TagLength = length;
        if (TagLength > 0x3e)
            longTag = true;
        else
            longTag = false;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Tag code property.
    /// </summary>
    public int TagCode
    {
        get => tagCode;
        set => tagCode = (ushort)value;
    }

    /// <summary>
    ///     Tag length property.
    /// </summary>
    public uint TagLength { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Writes binary data to given BinaryWriter.
    /// </summary>
    /// <param name="w">binary writer</param>
    public void WriteTo(BufferedBinaryWriter w)
    {
        if (longTag || TagLength > 0x3e)
        {
            var b = BitConverter.GetBytes(
                Convert.ToUInt16((Convert.ToUInt16(tagCode) << 6) + 0x3F)
            );
            w.Write(b);

            var len = TagLength;
            b = BitConverter.GetBytes(len);
            w.Write(b);
        }
        else
        {
            var b = BitConverter.GetBytes(
                Convert.ToUInt16((Convert.ToUInt16(tagCode) << 6) + Convert.ToUInt16(TagLength))
            );
            w.Write(b);
        }
    }

    /// <summary>
    ///     Reads the data from a binary file
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public void ReadData(BufferedBinaryReader binaryReader)
    {
        var tagCL = binaryReader.ReadUInt16();
        tagCode = Convert.ToUInt16(tagCL >> 6);
        TagLength = Convert.ToUInt32(tagCL - (tagCode << 6));

        bool longTag;

        if (TagLength == 0x3F)
        {
            var len = binaryReader.ReadUInt32();
            TagLength = len;
            longTag = TagLength <= 127;
        }
        else
            longTag = false;

        if (TagLength > binaryReader.BaseStream.Length) throw new InvalidTagLengthException();
    }

    #endregion
}