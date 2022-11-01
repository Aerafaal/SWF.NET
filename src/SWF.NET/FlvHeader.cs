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

namespace SWF.NET;

/// <summary>
///     FlvHeader.
/// </summary>
public class FlvHeader
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="FlvHeader" /> instance.
    /// </summary>
    public FlvHeader()
    {
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="reader">Reader.</param>
    public void ReadData(BufferedBinaryReader reader)
    {
        Signature = reader.ReadString(3);
        Version = reader.ReadByte();
        reader.ReadUBits(5);
        HasAudio = reader.ReadBoolean();
        reader.ReadBoolean();
        HasVideo = reader.ReadBoolean();
        reader.ReadUInt32();
    }

    #endregion

    #region Members

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets a value indicating whether this instance has video.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance has video; otherwise, <c>false</c>.
    /// </value>
    public bool HasVideo { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this instance has audio.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance has audio; otherwise, <c>false</c>.
    /// </value>
    public bool HasAudio { get; set; }

    /// <summary>
    ///     Gets or sets the signature.
    /// </summary>
    /// <value></value>
    public string Signature { get; set; }

    /// <summary>
    ///     Gets or sets the version.
    /// </summary>
    /// <value></value>
    public byte Version { get; set; }

    #endregion
}