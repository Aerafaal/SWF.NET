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

using SWF.NET.Tags.Types;
using SWF.NET.Utils;

namespace SWF.NET.Tags;

#region Enum

/// <summary>
///     FlvCodec
/// </summary>
public enum FlvCodec
{
	/// <summary>
	///     SorensonH263
	/// </summary>
	SorensonH263 = 2,

	/// <summary>
	///     ScreenVideo
	/// </summary>
	ScreenVideo = 3
}

/// <summary>
///     Frame Type
/// </summary>
public enum FlvFrameType
{
	/// <summary>
	///     KeyFrame
	/// </summary>
	KeyFrame = 1,

	/// <summary>
	///     InterFrame
	/// </summary>
	InterFrame = 2,

	/// <summary>
	///     DisposableInterFrame
	/// </summary>
	DisposableInterFrame = 3
}

#endregion

/// <summary>
///     FlvVideoTag.
/// </summary>
public class FlvVideoTag : FlvBaseTag
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="FlvVideoTag" /> instance.
    /// </summary>
    public FlvVideoTag()
    {
    }

    #endregion

    #region Members

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the codec.
    /// </summary>
    /// <value></value>
    public FlvCodec Codec { get; set; }

    /// <summary>
    ///     Gets or sets the frame type.
    /// </summary>
    /// <value></value>
    public FlvFrameType FrameType { get; set; }

    /// <summary>
    ///     Gets or sets the video data.
    /// </summary>
    /// <value></value>
    public VideoPacket VideoData { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="version">Version.</param>
    /// <param name="binaryReader">Binary reader.</param>
    public override void ReadData(byte version, BufferedBinaryReader binaryReader)
    {
        base.ReadData(version, binaryReader);
        Codec = (FlvCodec)binaryReader.ReadUBits(4);
        FrameType = (FlvFrameType)binaryReader.ReadUBits(4);
        if (Codec == FlvCodec.SorensonH263)
        {
            VideoData = new H263VideoPacket();
            VideoData.ReadData(binaryReader);
        }
        else
        {
            VideoData = new ScreenVideoPacket();
            VideoData.ReadData(binaryReader);
        }
    }

    /// <summary>
    ///     Updates the data.
    /// </summary>
    /// <param name="version">Version.</param>
    public override void UpdateData(byte version) =>
        base.UpdateData(version);

    #endregion
}