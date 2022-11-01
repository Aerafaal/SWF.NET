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

#region Enum

/// <summary>
///     Flv Sound Format
/// </summary>
public enum FlvSoundFormat
{
	/// <summary>
	///     Uncompressed
	/// </summary>
	Uncompressed = 0,

	/// <summary>
	///     ADPCM
	/// </summary>
	ADPCM = 1,

	/// <summary>
	///     Mp3
	/// </summary>
	Mp3 = 2,

	/// <summary>
	///     NellyMoser8KhzMono
	/// </summary>
	NellyMoser8KhzMono = 5,

	/// <summary>
	///     NellyMoser
	/// </summary>
	NellyMoser = 6
}

#endregion

/// <summary>
///     FlvAudioTag.
/// </summary>
public class FlvAudioTag : FlvBaseTag
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="FlvAudioTag" /> instance.
    /// </summary>
    public FlvAudioTag()
    {
    }

    #endregion

    #region Members

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the sound format.
    /// </summary>
    /// <value></value>
    public FlvSoundFormat SoundFormat { get; set; }

    /// <summary>
    ///     Gets or sets the sound rate.
    /// </summary>
    /// <value></value>
    public uint SoundRate { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this instance is SND16 bits.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is SND16 bits; otherwise, <c>false</c>.
    /// </value>
    public bool IsSnd16Bits { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this instance is stereo.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is stereo; otherwise, <c>false</c>.
    /// </value>
    public bool IsStereo { get; set; }

    /// <summary>
    ///     Gets or sets the sound data.
    /// </summary>
    /// <value></value>
    public byte[] SoundData { get; set; }

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
        SoundFormat = (FlvSoundFormat)binaryReader.ReadUBits(4);
        SoundRate = binaryReader.ReadUBits(2);
        IsSnd16Bits = binaryReader.ReadBoolean();
        IsStereo = binaryReader.ReadBoolean();

        var dataLenght = dataSize - 1;
        if (dataLenght > 0)
        {
            SoundData = new byte[dataLenght];
            for (var i = 0; i < dataLenght; i++)
                SoundData[i] = binaryReader.ReadByte();
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