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
using System.Xml;
using SWF.NET.Utils;

namespace SWF.NET.Tags;

/// <summary>
///     SoundStreamHead2Tag defines an sound compressed using different
///     compression formats that is streamed in tight synchronisation
///     with the movie being played.
/// </summary>
/// <remarks>
///     <p>
///     </p>
/// </remarks>
public class SoundStreamHead2Tag : BaseTag
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="SoundStreamHead2Tag" /> instance.
    /// </summary>
    public SoundStreamHead2Tag() =>
        _tagCode = (int)TagCodeEnum.SoundStreamHead2;

    /// <summary>
    ///     Creates a new <see cref="SoundStreamHead2Tag" /> instance.
    /// </summary>
    /// <param name="playbackSoundRate">Playback sound rate.</param>
    /// <param name="playbackSoundSize">Size of the playback sound.</param>
    /// <param name="playbackSoundType">Playback sound type.</param>
    /// <param name="streamSoundCompression">Stream sound compression.</param>
    /// <param name="streamSoundRate">Stream sound rate.</param>
    /// <param name="streamSoundSize">Size of the stream sound.</param>
    /// <param name="streamSoundType">Stream sound type.</param>
    /// <param name="streamSoundSampleCount">Stream sound sample count.</param>
    /// <param name="latencySeek">Latency seek.</param>
    public SoundStreamHead2Tag(
        uint playbackSoundRate,
        uint playbackSoundSize,
        uint playbackSoundType,
        uint streamSoundCompression,
        uint streamSoundRate,
        uint streamSoundSize,
        uint streamSoundType,
        ushort streamSoundSampleCount,
        short latencySeek)
    {
        this.PlaybackSoundRate = playbackSoundRate;
        this.PlaybackSoundSize = playbackSoundSize;
        this.PlaybackSoundType = playbackSoundType;
        this.StreamSoundCompression = streamSoundCompression;
        this.StreamSoundRate = streamSoundRate;
        this.StreamSoundSize = streamSoundSize;
        this.StreamSoundType = streamSoundType;
        this.StreamSoundSampleCount = streamSoundSampleCount;
        this.LatencySeek = latencySeek;
        _tagCode = (int)TagCodeEnum.SoundStreamHead2;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the playback sound rate.
    /// </summary>
    public uint PlaybackSoundRate { get; set; }

    /// <summary>
    ///     Gets or sets the size of the playback sound.
    /// </summary>
    public uint PlaybackSoundSize { get; set; }

    /// <summary>
    ///     Gets or sets the playback sound type.
    /// </summary>
    public uint PlaybackSoundType { get; set; }

    /// <summary>
    ///     Gets or sets the stream sound compression.
    /// </summary>
    public uint StreamSoundCompression { get; set; }

    /// <summary>
    ///     Gets or sets the stream sound rate.
    /// </summary>
    public uint StreamSoundRate { get; set; }

    /// <summary>
    ///     Gets or sets the size of the stream sound.
    /// </summary>
    public uint StreamSoundSize { get; set; }

    /// <summary>
    ///     Gets or sets the stream sound type.
    /// </summary>
    public uint StreamSoundType { get; set; }

    /// <summary>
    ///     Gets or sets the stream sound sample count.
    /// </summary>
    public ushort StreamSoundSampleCount { get; set; }

    /// <summary>
    ///     Gets or sets the latency seek.
    /// </summary>
    public short LatencySeek { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void ReadData(byte version, BufferedBinaryReader binaryReader)
    {
        var rh = new RecordHeader();
        rh.ReadData(binaryReader);

        binaryReader.ReadUBits(4);
        PlaybackSoundRate = binaryReader.ReadUBits(2);
        PlaybackSoundSize = binaryReader.ReadUBits(1);
        PlaybackSoundType = binaryReader.ReadUBits(1);
        StreamSoundCompression = binaryReader.ReadUBits(4);
        StreamSoundRate = binaryReader.ReadUBits(2);
        StreamSoundSize = binaryReader.ReadUBits(1);
        StreamSoundType = binaryReader.ReadUBits(1);

        StreamSoundSampleCount = binaryReader.ReadUInt16();
        LatencySeek = 0;

        if (StreamSoundCompression == 2)
            LatencySeek = binaryReader.ReadInt16();
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf()
    {
        var res = 4;
        if (StreamSoundCompression == 2)
            res += 2;
        return res;
    }

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void UpdateData(byte version)
    {
        var m = new MemoryStream();
        var w = new BufferedBinaryWriter(m);

        var rh = new RecordHeader(TagCode, GetSizeOf());
        rh.WriteTo(w);

        w.WriteUBits(0, 4);
        w.WriteUBits(PlaybackSoundRate, 2);
        w.WriteUBits(PlaybackSoundSize, 1);
        w.WriteUBits(PlaybackSoundType, 1);
        w.SynchBits();
        w.WriteUBits(StreamSoundCompression, 4);
        w.WriteUBits(StreamSoundRate, 2);
        w.WriteUBits(StreamSoundSize, 1);
        w.WriteUBits(StreamSoundType, 1);

        w.Write(StreamSoundSampleCount);
        if (StreamSoundCompression == 2)
            w.Write(LatencySeek);

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
        writer.WriteStartElement("SoundStreamHeadTag");

        writer.WriteElementString("PlaybackSoundRate", PlaybackSoundRate.ToString());
        writer.WriteElementString("PlaybackSoundSize", PlaybackSoundSize.ToString());
        writer.WriteElementString("PlaybackSoundType", PlaybackSoundType.ToString());
        writer.WriteElementString("StreamSoundCompression", StreamSoundCompression.ToString());
        writer.WriteElementString("StreamSoundRate", StreamSoundRate.ToString());
        writer.WriteElementString("StreamSoundSize", StreamSoundSize.ToString());
        writer.WriteElementString("StreamSoundType", StreamSoundType.ToString());
        writer.WriteElementString("StreamSoundSampleCount", StreamSoundSampleCount.ToString());
        if (StreamSoundCompression == 2)
            writer.WriteElementString("LatencySeek", LatencySeek.ToString());

        writer.WriteEndElement();
    }

    #endregion
}