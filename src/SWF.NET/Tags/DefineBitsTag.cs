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

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;
using SWF.NET.Utils;

namespace SWF.NET.Tags;

/// <summary>
///     DefineBits tag for Jpeg images in swf
/// </summary>
public class DefineBitsTag : BaseTag, DefineTag
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="DefineBitsTag" /> instance.
    /// </summary>
    public DefineBitsTag() =>
        _tagCode = (int)TagCodeEnum.DefineBits;

    /// <summary>
    ///     Creates a new <see cref="DefineBitsTag" /> instance.
    /// </summary>
    /// <param name="id">Id.</param>
    /// <param name="image">Image.</param>
    public DefineBitsTag(ushort id, byte[] image)
    {
        CharacterId = id;
        JpegData = image;
        _tagCode = (int)TagCodeEnum.DefineBits;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     JPEG Data
    /// </summary>
    public byte[] JpegData { get; set; }

    /// <summary>
    ///     see <see cref="DefineTag" />
    /// </summary>
    public ushort CharacterId { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void ReadData(byte version, BufferedBinaryReader binaryReader)
    {
        var rh = new RecordHeader();
        rh.ReadData(binaryReader);

        var tl = Convert.ToInt32(rh.TagLength);
        CharacterId = binaryReader.ReadUInt16();
        JpegData = binaryReader.ReadBytes(tl - 2);
    }

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void UpdateData(byte version)
    {
        var m = new MemoryStream();
        var w = new BufferedBinaryWriter(m);

        var rh = new RecordHeader(TagCode, 2 + JpegData.Length);

        rh.WriteTo(w);
        w.Write(CharacterId);
        w.Write(JpegData);

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
        writer.WriteStartElement("DefineBitsTag");
        writer.WriteAttributeString("CharacterId", CharacterId.ToString());
        if (JpegData != null)
            writer.WriteAttributeString("JpegDataLenght", JpegData.Length.ToString());
        writer.WriteEndElement();
    }

    #endregion

    #region Methods: Compile & Decompile

    /// <summary>
    ///     Construct a new DefineBitsJpeg2Tag object
    ///     from a file.
    /// </summary>
    /// <param name="characterId">Character id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static DefineBitsTag FromFile(ushort characterId, string fileName)
    {
        var stream = File.OpenRead(fileName);
        var res = FromStream(characterId, stream);
        stream.Close();
        return res;
    }

    /// <summary>
    ///     Construct a new DefineBitsJpeg2Tag object
    ///     from a stream.
    /// </summary>
    /// <param name="characterId">Character id.</param>
    /// <param name="stream">Stream.</param>
    /// <returns></returns>
    public static DefineBitsTag FromStream(ushort characterId, Stream stream)
    {
        var jpegTag = new DefineBitsTag();
        jpegTag.CharacterId = characterId;

        var buffer = new byte[(int)stream.Length];
        stream.Read(buffer, 0, (int)stream.Length);

        var buffer2 = new byte[buffer.Length + 4];
        buffer2[0] = 0xFF;
        buffer2[1] = 0xD9;
        buffer2[2] = 0xFF;
        buffer2[3] = 0xD8;
        for (var i = 0; i < buffer.Length; i++)
            buffer2[i + 4] = buffer[i];

        jpegTag.JpegData = buffer2;

        return jpegTag;
    }

    /// <summary>
    ///     Construct a new DefineBitsJpeg2Tag object
    ///     from an image object.
    /// </summary>
    /// <param name="characterId">Character id.</param>
    /// <param name="image">Image.</param>
    /// <returns></returns>
    public static DefineBitsTag FromImage(ushort characterId, Image image)
    {
        if (image == null)
            return null;

        var ms = new MemoryStream();
        image.Save(ms, ImageFormat.Jpeg);
        var buffer = ms.GetBuffer();
        ms.Close();

        var stream = new MemoryStream(buffer);
        return FromStream(characterId, stream);
    }

    #endregion
}