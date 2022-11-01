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
using SWF.NET.Tags.Types;
using SWF.NET.Utils;

namespace SWF.NET.Tags;

/// <summary>
///     StartSound tag instructs the player to start or stop
///     playing a sound defined using the DefineSoundTag class.
/// </summary>
/// <remarks>
///     <p>
///     </p>
///     <p>
///         This tag was introduced in Flash 1.
///     </p>
/// </remarks>
public class StartSoundTag : BaseTag
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="StartSoundTag" /> instance.
    /// </summary>
    public StartSoundTag() =>
        _tagCode = (int)TagCodeEnum.StartSound;

    /// <summary>
    ///     Creates a new <see cref="StartSoundTag" /> instance.
    /// </summary>
    /// <param name="soundId">Sound id.</param>
    /// <param name="soundInfo">Sound info.</param>
    public StartSoundTag(ushort soundId, SoundInfo soundInfo)
    {
        SoundId = soundId;
        SoundInfo = soundInfo;
        _tagCode = (int)TagCodeEnum.StartSound;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the sound id.
    /// </summary>
    /// <value></value>
    public ushort SoundId { get; set; }

    /// <summary>
    ///     Gets or sets the sound info.
    /// </summary>
    /// <value></value>
    public SoundInfo SoundInfo { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void ReadData(byte version, BufferedBinaryReader binaryReader)
    {
        var rh = new RecordHeader();
        rh.ReadData(binaryReader);

        SoundId = binaryReader.ReadUInt16();
        SoundInfo = new SoundInfo();
        SoundInfo.ReadData(binaryReader);
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf()
    {
        var length = 2;
        if (SoundInfo != null)
            length += SoundInfo.GetSizeOf();
        return length;
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

        w.Write(SoundId);
        if (SoundInfo != null)
            SoundInfo.WriteTo(w);

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
        writer.WriteStartElement("StartSound");
        writer.WriteElementString("SoundId", SoundId.ToString());
        if (SoundInfo != null)
            SoundInfo.Serialize(writer);
        writer.WriteEndElement();
    }

    #endregion
}