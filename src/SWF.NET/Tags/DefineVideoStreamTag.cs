﻿/*
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

using System.IO;
using System.Xml;
using SWF.NET.Utils;

namespace SWF.NET.Tags;

/// <summary>
///     DefineVideoStreamTag class is used to display
///     video within a Flash file.
/// </summary>
/// <remarks>
///     <p>
///         Video objects contain a unique identifier and are treated
///         in the same way as shapes, buttons, images, etc.
///         The video data displayed is define using the VideoFrameTag
///         class. Each frame of video is displayed whenever display
///         list is updated using the ShowFrameTag object -
///         any timing information stored within the video data
///         is ignored.
///     </p>
///     <p>
///         This tag was introduced in Flash 6. The ScreenVideo
///         format was introduced in Flash 7.
///     </p>
/// </remarks>
public class DefineVideoStreamTag : BaseTag, DefineTag
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="DefineVideoStreamTag" /> instance.
    /// </summary>
    public DefineVideoStreamTag() =>
        _tagCode = (int)TagCodeEnum.DefineVideoStream;

    /// <summary>
    ///     Creates a new <see cref="DefineVideoStreamTag" /> instance.
    /// </summary>
    /// <param name="characterId">Character id.</param>
    /// <param name="numFrames">Num frames.</param>
    /// <param name="width">Width of each frame in pixels.</param>
    /// <param name="height">Height of each frame in pixels.</param>
    /// <param name="videoFlagsDeblocking">Video flags deblocking.</param>
    /// <param name="videoFlagsSmoothing">Video flags smoothing.</param>
    /// <param name="codecId">Codec id.</param>
    public DefineVideoStreamTag(
        ushort characterId,
        ushort numFrames,
        ushort width,
        ushort height,
        uint videoFlagsDeblocking,
        bool videoFlagsSmoothing,
        byte codecId)
    {
        this.CharacterId = characterId;
        this.NumFrames = numFrames;
        this.Width = width;
        this.Height = height;
        this.Deblocking = videoFlagsDeblocking;
        this.Smoothing = videoFlagsSmoothing;
        this.CodecId = codecId;
        _tagCode = (int)TagCodeEnum.DefineVideoStream;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the num frames.
    ///     The number of frames that will be displayed in this stream.
    /// </summary>
    public ushort NumFrames { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether
    ///     this <see cref="DefineVideoStreamTag" /> is smoothing.
    ///     Controls whether the Plash Player performs smoothing
    ///     to increase the quality of the image displayed albeit
    ///     at the price of performance.
    /// </summary>
    public bool Smoothing { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether
    ///     this <see cref="DefineVideoStreamTag" /> is deblocking.
    ///     Whether a filter is used when assembling the blocks of video
    ///     data into a frame. This may be set to Off to turn off the
    ///     deblock filter in the Flash Player; On to whether the
    ///     deblocking filter is used.
    /// </summary>
    public uint Deblocking { get; set; }

    /// <summary>
    ///     Gets or sets the character id.
    ///     That's a unique identifier, in the range 1..65535, that
    ///     is used to reference the video from other objects, e.g.
    ///     when adding or removing from the display list.
    /// </summary>
    public ushort CharacterId { get; set; }

    /// <summary>
    ///     Gets or sets the codec id.
    ///     Identifies the format of the video data either
    ///     H263VideoPacket for data encoded using the Sorenson
    ///     modified H263 format or ScreenVideoPacket for data encoded
    ///     using Macromedia's Screen Video format.
    ///     2 = Sorenson H263, 3 = ScreenVideo (SWF 7 or +)
    /// </summary>
    public byte CodecId { get; set; }

    /// <summary>
    ///     Gets or sets the width of each frame in pixels.
    /// </summary>
    public ushort Width { get; set; }

    /// <summary>
    ///     Gets or sets the height of each frame in pixels.
    /// </summary>
    public ushort Height { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void ReadData(byte version, BufferedBinaryReader binaryReader)
    {
        var rh = new RecordHeader();
        rh.ReadData(binaryReader);

        CharacterId = binaryReader.ReadUInt16();
        NumFrames = binaryReader.ReadUInt16();
        Width = binaryReader.ReadUInt16();
        Height = binaryReader.ReadUInt16();
        binaryReader.ReadUBits(5);
        Deblocking = binaryReader.ReadUBits(2);
        Smoothing = binaryReader.ReadBoolean();
        CodecId = binaryReader.ReadByte();
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf() =>
        10;

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void UpdateData(byte version)
    {
        if (version < 6)
            return;

        var m = new MemoryStream();
        var w = new BufferedBinaryWriter(m);

        var rh = new RecordHeader(TagCode, GetSizeOf());
        rh.WriteTo(w);

        w.Write(CharacterId);
        w.Write(NumFrames);
        w.Write(Width);
        w.Write(Height);

        w.WriteUBits(0, 5);
        w.WriteUBits(Deblocking, 2);
        w.WriteBoolean(Smoothing);

        w.Write(CodecId);

        w.Flush();
        // write to data array
        _data = m.ToArray();
    }


    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("DefineVideoStreamTag");
        writer.WriteAttributeString("CharacterId", CharacterId.ToString());
        writer.WriteElementString("NumFrames", NumFrames.ToString());
        writer.WriteElementString("Width", Width.ToString());
        writer.WriteElementString("Height", Height.ToString());
        writer.WriteElementString("VideoFlagsDeblocking", Deblocking.ToString());
        writer.WriteElementString("VideoFlagsSmoothing", Smoothing.ToString());
        writer.WriteElementString("CodecId", CodecId.ToString());
        writer.WriteEndElement();
    }

    #endregion
}