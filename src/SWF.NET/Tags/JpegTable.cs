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

using System;
using System.IO;
using System.Xml;
using SWF.NET.Utils;

namespace SWF.NET.Tags;

/// <summary>
///     JPEGTableTag defines an encoding table for JPEG images.
/// </summary>
/// <remarks>
///     <p>
///         The encoding table is shared between all images defined
///         using the DefineBitsTag class so there should only
///         be one JpegTableTag object defined in a movie.
///     </p>
///     <p>
///         The JpegTableTag class is not essential to define
///         JPEG encoded images in a movie using the DefineBitsJepg2Tag class.
///         If an JpegTableTag object is created with an empty encoding
///         table then the Flash Player will still display JPEG images
///         defined using DefineBitsTag objects correctly. When an
///         JpegTableTag with an empty encoding table is encoded to a
///         Flash file, the "end of stream" marker 0xFFD9 is encoded
///         allowing the empty table to be decoded correctly.
///     </p>
///     <p>
///         The simplest way to use the JpegTableTag and DefineBitsTag
///         classes to define JPEG encoded images is to create an empty
///         encoding table then construct the DefineBitsTag object(s)
///         with the image data from a file:
///         <code lang="C#">
/// 
/// </code>
///     </p>
///     <p>
///         This tag was introduced in Flash 1.
///     </p>
/// </remarks>
public class JpegTableTag : BaseTag
{
    #region Members

    #endregion

    #region Properties

    /// <summary>
    ///     JPEG Data is an array of bytes containing the
    ///     encoding table data.
    /// </summary>
    public byte[] JpegData { get; set; }

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="JpegTableTag" /> instance.
    /// </summary>
    public JpegTableTag() =>
        _tagCode = (int)TagCodeEnum.JpegTable;

    /// <summary>
    ///     Creates a new <see cref="JpegTableTag" /> instance.
    /// </summary>
    /// <param name="jpeg">JPEG data. An array of bytes containing the encoding table data.</param>
    public JpegTableTag(byte[] jpeg)
    {
        JpegData = jpeg;
        _tagCode = (int)TagCodeEnum.JpegTable;
    }

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
        JpegData = binaryReader.ReadBytes(tl);
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>Size of this object.</returns>
    protected int GetSizeOf()
    {
        var res = 0;
        if (JpegData != null)
            res += JpegData.Length;
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
        if (JpegData != null)
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
        writer.WriteStartElement("JpegTable");
        if (JpegData != null)
            writer.WriteAttributeString("JpegDataLenght", JpegData.Length.ToString());
        writer.WriteEndElement();
    }

    #endregion
}