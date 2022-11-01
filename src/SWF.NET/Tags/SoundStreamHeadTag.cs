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
///     SoundStreamHead tag defines the format of a streaming
///     sound, identifying the encoding scheme, the rate at which the
///     sound will be played and the size of the decoded samples.
/// </summary>
/// <remarks>
///     <p>
///         The actual sound is streamed used the SoundStreamBlockTag
///         class which contains the data for each frame in a movie.
///     </p>
///     <p>
///         Three encoded formats for the sound data are supported:
///         <ul>
///             <li>
///                 NATIVE_PCM - uncompressed Pulse Code Modulated: samples
///                 are either 1 or 2 bytes. For two-byte samples the byte order
///                 is dependent on the platform on which the Flash Player is hosted.
///                 Sounds created on a platform which supports big-endian byte
///                 order will not be played correctly when listened to on a platform
///                 which supports little-endian byte order.
///             </li>
///             <li>
///                 PCM - uncompressed Pulse Code Modulated: samples are
///                 either 1 or 2 bytes with the latter presented in Little-Endian
///                 byte order. This ensures that sounds can be played across
///                 different platforms.
///             </li>
///             <li>
///                 ADPCM - compressed ADaptive Pulse Code Modulated: samples
///                 are encoded and compressed by comparing the difference between
///                 successive sound sample which dramatically reduces the size of
///                 the encoded sound when compared to the uncompressed PCM formats.
///                 Use this format whenever possible.
///             </li>
///         </ul>
///     </p>
///     <p>
///         When a stream sound is played if the Flash Player cannot render
///         the frames fast enough to maintain synchronisation with the sound
///         being played then frames will be skipped. Normally the player will
///         reduce the frame rate so every frame of a movie is played.
///         The different sets of attributes that identify how the sound will
///         be played compared to the way it was encoded allows the Player
///         more control over how the animation is rendered. Reducing the
///         resolution or playback rate can improve synchronization with
///         the frames displayed.
///     </p>
///     <p>
///         This tag was introduced in Flash 1.
///     </p>
/// </remarks>
public class SoundStreamHeadTag : BaseTag
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="SoundStreamHeadTag" /> instance.
    /// </summary>
    public SoundStreamHeadTag() =>
        _tagCode = (int)TagCodeEnum.SoundStreamHead;

    /// <summary>
    ///     Creates a new <see cref="SoundStreamHeadTag" /> instance.
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
    public SoundStreamHeadTag(
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
        _tagCode = (int)TagCodeEnum.SoundStreamHead;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the playback sound rate.
    ///     The recommended playback rate in Hertz :
    ///     0 = 5512, 1 = 11025, 2 = 22050 or 3 = 44100.
    /// </summary>
    public uint PlaybackSoundRate { get; set; }

    /// <summary>
    ///     Gets or sets the size of the playback sound.
    ///     The number of bytes in an uncompressed sample
    ///     when the sound is played, 1 = 16 bits.
    /// </summary>
    public uint PlaybackSoundSize { get; set; }

    /// <summary>
    ///     Gets or sets the playback sound type.
    ///     The recommended number of playback
    ///     channels: 0 = mono or 1 = stereo.
    /// </summary>
    public uint PlaybackSoundType { get; set; }

    /// <summary>
    ///     Gets or sets the stream sound compression.
    ///     Format of streaming sound data: 1 = ADPCM
    ///     or 2 = MP3 (for SWF 4 or +)
    /// </summary>
    public uint StreamSoundCompression { get; set; }

    /// <summary>
    ///     Gets or sets the stream sound rate.
    ///     The rate at which the streaming sound was
    ///     samples - 0 = 5512, 1 = 11025, 2 = 22050
    ///     or 3 = 44100 Hz
    /// </summary>
    public uint StreamSoundRate { get; set; }

    /// <summary>
    ///     Gets or sets the size of the stream sound.
    ///     The size of an uncompressed sample in the
    ///     streaming sound in bytes, 1 = 16 bits.
    /// </summary>
    public uint StreamSoundSize { get; set; }

    /// <summary>
    ///     Gets or sets the stream sound type.
    ///     The number of channels: 0 = mono or 1 = stereo,
    ///     in the streaming sound.
    /// </summary>
    public uint StreamSoundType { get; set; }

    /// <summary>
    ///     Gets or sets the stream sound sample count.
    ///     The average number of samples in each
    ///     SoundStreamBlockTag object.
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