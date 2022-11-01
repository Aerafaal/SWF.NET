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
///     SetBackgroundColorTag object defines a background color
///     of the swf file. It sets the background color displayed
///     in every frame in the movie.
/// </summary>
/// <remarks>
///     <p>
///         Although the color is specified using an RGB object the
///         colour displayed is completely opaque.
///     </p>
///     <p>
///         The background color must be set before the first frame is
///         displayed otherwise the background color defaults to white.
///         This is typically the first object in a coder.
///         If more than one SetBackgroundColorTag object is added to a
///         swf then only first one sets the background color.
///         Subsequent objects are ignored.
///     </p>
/// </remarks>
/// <example>
///     <code lang="C#">
/// Swf swf = new Swf();
/// swf.FrameSize = new Rect(0, 0, 8000, 8000)); // in twips = 400 x 400 in pixels
/// swf.FrameRate = 1.0; 1 frame per second.
/// swf.Add(new SetBackgroundColorTag(new RGB(0, 0, 255))); // Blue
/// </code>
/// </example>
public class SetBackgroundColorTag : BaseTag
{
    #region Members

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the RGB color.
    /// </summary>
    public RGB RGB { get; set; }

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="SetBackgroundColorTag" /> instance.
    /// </summary>
    public SetBackgroundColorTag() =>
        _tagCode = (int)TagCodeEnum.SetBackgroundColor;

    /// <summary>
    ///     Creates a new <see cref="SetBackgroundColorTag" /> instance.
    /// </summary>
    /// <param name="rgbColor">Color of the RGB.</param>
    public SetBackgroundColorTag(RGB rgbColor)
    {
        RGB = rgbColor;
        _tagCode = (int)TagCodeEnum.SetBackgroundColor;
    }

    /// <summary>
    ///     Creates a new <see cref="SetBackgroundColorTag" /> instance.
    /// </summary>
    /// <param name="red">Red.</param>
    /// <param name="green">Green.</param>
    /// <param name="blue">Blue.</param>
    public SetBackgroundColorTag(byte red, byte green, byte blue)
    {
        RGB = new RGB(red, green, blue);
        _tagCode = (int)TagCodeEnum.SetBackgroundColor;
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

        RGB = new RGB();
        RGB.ReadData(binaryReader);
    }

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void UpdateData(byte version)
    {
        var m = new MemoryStream();
        var w = new BufferedBinaryWriter(m);

        var rh = new RecordHeader(TagCode, RGB.GetSizeOf());
        rh.WriteTo(w);
        RGB.WriteTo(w);

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
        writer.WriteStartElement("SetBackgroundColorTag");
        RGB.Serialize(writer);
        writer.WriteEndElement();
    }

    #endregion
}