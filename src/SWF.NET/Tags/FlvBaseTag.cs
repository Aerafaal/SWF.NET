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

using SWF.NET.Utils;

namespace SWF.NET.Tags;

/// <summary>
///     FlvBaseTag.
/// </summary>
public class FlvBaseTag
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="FlvBaseTag" /> instance.
    /// </summary>
    public FlvBaseTag()
    {
    }

    #endregion

    #region Members

    /// <summary>
    ///     Tag type
    /// </summary>
    protected FlvTagCodeEnum tagType;

    /// <summary>
    ///     Size of the tag
    /// </summary>
    protected uint dataSize;

    /// <summary>
    ///     Timestamp
    /// </summary>
    protected uint timeStamp;

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the time stamp.
    /// </summary>
    /// <value></value>
    public uint TimeStamp
    {
        get => timeStamp;
        set => timeStamp = value;
    }

    /// <summary>
    ///     Gets or sets the tag type.
    /// </summary>
    /// <value></value>
    public FlvTagCodeEnum TagType
    {
        get => tagType;
        set => tagType = value;
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="version">Version.</param>
    /// <param name="binaryReader">Binary reader.</param>
    public virtual void ReadData(byte version, BufferedBinaryReader binaryReader)
    {
        tagType = (FlvTagCodeEnum)binaryReader.ReadByte();
        dataSize = binaryReader.ReadUBits(24);
        timeStamp = binaryReader.ReadUBits(24);
        binaryReader.ReadUInt32();
    }

    /// <summary>
    ///     Updates the data.
    /// </summary>
    /// <param name="version">Version.</param>
    public virtual void UpdateData(byte version)
    {
    }

    #endregion
}