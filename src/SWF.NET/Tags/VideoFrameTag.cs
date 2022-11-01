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

using System.IO;
using SWF.NET.Tags.Types;
using SWF.NET.Utils;

namespace SWF.NET.Tags;

/// <summary>
///     VideoFrameTag contains the video data displayed in a
///     single frame of a Flash movie.
/// </summary>
/// <remarks>
///     <p>
///         Each frame of video is displayed whenever display list is
///         updated using the ShowFrameTag object - any timing
///         information stored within the video data is ignored.
///         Since the video is updated at the same time as the display
///         list the frame rate of the video may be the same or less
///         than the frame rate of the Flash movie but not higher.
///     </p>
///     <p>
///         This tag was added in Flash 6 with support for the Sorenson
///         modified H263 format. Support for Macromedia's Screen Video
///         format was added in Flash 7.
///     </p>
/// </remarks>
public class VideoFrameTag : BaseTag
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="VideoFrameTag" /> instance.
    /// </summary>
    public VideoFrameTag() =>
        _tagCode = (int)TagCodeEnum.VideoFrame;

    /// <summary>
    ///     Creates a new <see cref="VideoFrameTag" /> instance.
    /// </summary>
    /// <param name="streamId">Stream id.</param>
    /// <param name="frameNum">Frame num.</param>
    /// <param name="video">Video.</param>
    public VideoFrameTag(ushort streamId, ushort frameNum, H263VideoPacket video)
    {
        _tagCode = (int)TagCodeEnum.VideoFrame;
        StreamId = streamId;
        FrameNum = frameNum;
        VideoPacket = video;
    }

    /// <summary>
    ///     Creates a new <see cref="VideoFrameTag" /> instance.
    /// </summary>
    /// <param name="streamId">Stream id.</param>
    /// <param name="frameNum">Frame num.</param>
    /// <param name="video">Video.</param>
    public VideoFrameTag(ushort streamId, ushort frameNum, ScreenVideoPacket video)
    {
        _tagCode = (int)TagCodeEnum.VideoFrame;
        StreamId = streamId;
        FrameNum = frameNum;
        VideoPacket = video;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the stream id.
    /// </summary>
    public ushort StreamId { get; set; }

    /// <summary>
    ///     Gets or sets the frame num.
    /// </summary>
    public ushort FrameNum { get; set; }

    /// <summary>
    ///     Gets or sets the H263 video packet.
    /// </summary>
    public VideoPacket VideoPacket { get; set; }

    /// <summary>
    ///     Gets or sets the codec id.
    /// </summary>
    /// <value></value>
    public ushort CodecId { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void ReadData(byte version, BufferedBinaryReader binaryReader)
    {
        var rh = new RecordHeader();
        rh.ReadData(binaryReader);

        StreamId = binaryReader.ReadUInt16();
        FrameNum = binaryReader.ReadUInt16();

        if (CodecId == 2)
        {
            VideoPacket = new H263VideoPacket();
            VideoPacket.ReadData(binaryReader);
        }
        else if (CodecId == 3)
        {
            VideoPacket = new ScreenVideoPacket();
            VideoPacket.ReadData(binaryReader);
        }
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf()
    {
        var res = 4;
        if (VideoPacket != null)
            res += VideoPacket.GetSizeOf();
        return res;
    }

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
        w.Write(StreamId);
        w.Write(FrameNum);
        if (VideoPacket != null)
            VideoPacket.WriteTo(w);

        // write to data array
        w.Flush();
        _data = m.ToArray();
    }

    #endregion
}