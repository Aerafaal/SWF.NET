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

using System.Drawing;
using System.IO;
using System.Xml;
using SWF.NET.Utils;

namespace SWF.NET.Tags.Types;

/// <summary>
///     RGBColor class
/// </summary>
public abstract class RGBColor : ISwfSerializer
{
    #region Members

    /// <summary>
    ///     Red
    /// </summary>
    public byte red;

    /// <summary>
    ///     Green
    /// </summary>
    public byte green;

    /// <summary>
    ///     Blue
    /// </summary>
    public byte blue;

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="RGBColor" /> instance.
    /// </summary>
    /// <param name="red">Red.</param>
    /// <param name="green">Green.</param>
    /// <param name="blue">Blue.</param>
    protected RGBColor(byte red, byte green, byte blue)
    {
        this.red = red;
        this.green = green;
        this.blue = blue;
    }

    /// <summary>
    ///     Creates a new <see cref="RGBColor" /> instance.
    /// </summary>
    protected RGBColor()
    {
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    public abstract int GetSizeOf();

    /// <summary>
    ///     Writes to a binary writer
    /// </summary>
    /// <param name="writer">Writer.</param>
    public virtual void WriteTo(BinaryWriter writer)
    {
        writer.Write(red);
        writer.Write(green);
        writer.Write(blue);
    }

    /// <summary>
    ///     Reads the data from the binary reader.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public virtual void ReadData(BufferedBinaryReader binaryReader)
    {
        red = binaryReader.ReadByte();
        green = binaryReader.ReadByte();
        blue = binaryReader.ReadByte();
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public virtual void Serialize(XmlWriter writer)
    {
    }

    /// <summary>
    ///     Gets a RGB or RGBA object from a windows color.
    /// </summary>
    /// <param name="color">Color.</param>
    /// <returns></returns>
    public static RGBColor FromWinColor(Color color)
    {
        if (color.A != 255)
            return new RGBA(color.R, color.G, color.B, color.A);
        return new RGB(color.R, color.G, color.B);
    }

    #endregion
}

/// <summary>
///     RGB
/// </summary>
public class RGB : RGBColor
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="RGB" /> instance.
    /// </summary>
    /// <param name="red">Red.</param>
    /// <param name="green">Green.</param>
    /// <param name="blue">Blue.</param>
    public RGB(byte red, byte green, byte blue) :
        base(red, green, blue)
    {
    }

    /// <summary>
    ///     Creates a new <see cref="RGB" /> instance.
    /// </summary>
    public RGB()
    {
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    public override int GetSizeOf() =>
        3;

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void WriteTo(BinaryWriter writer) =>
        base.WriteTo(writer);

    /// <summary>
    ///     Reads the data from a binary reader.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public override void ReadData(BufferedBinaryReader binaryReader) =>
        base.ReadData(binaryReader);

    /// <summary>
    ///     Gets a RGB object from a windows color.
    /// </summary>
    /// <param name="color">Color.</param>
    /// <returns></returns>
    public new static RGB FromWinColor(Color color) =>
        new(color.R, color.G, color.B);

    /// <summary>
    ///     Transform a RGB object to a win color object
    /// </summary>
    /// <returns>GDI Color formated</returns>
    public Color ToWinColor() =>
        Color.FromArgb(red, green, blue);

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("RGB");
        writer.WriteAttributeString("Red", red.ToString());
        writer.WriteAttributeString("Green", green.ToString());
        writer.WriteAttributeString("Blue", blue.ToString());
        writer.WriteEndElement();
    }

    #endregion
}

/// <summary>
///     RGBA
/// </summary>
public class RGBA : RGBColor
{
    #region Members

    /// <summary>
    ///     Alpha
    /// </summary>
    public byte alpha;

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="RGBA" /> instance.
    /// </summary>
    /// <param name="red">Red.</param>
    /// <param name="green">Green.</param>
    /// <param name="blue">Blue.</param>
    /// <param name="alpha">Alpha.</param>
    public RGBA(byte red, byte green, byte blue, byte alpha) :
        base(red, green, blue) =>
        this.alpha = alpha;

    /// <summary>
    ///     Creates a new <see cref="RGBA" /> instance.
    /// </summary>
    public RGBA()
    {
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    public override int GetSizeOf() =>
        4;

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void WriteTo(BinaryWriter writer)
    {
        base.WriteTo(writer);
        writer.Write(alpha);
    }

    /// <summary>
    ///     Reads the data from a binary reader.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public override void ReadData(BufferedBinaryReader binaryReader)
    {
        base.ReadData(binaryReader);
        alpha = binaryReader.ReadByte();
    }

    /// <summary>
    ///     Transform a RGBA color to a win color
    /// </summary>
    /// <returns>GDI Color formated</returns>
    public Color ToWinColor() =>
        Color.FromArgb(alpha, red, green, blue);

    /// <summary>
    ///     Gets a RGB object from a windows color.
    /// </summary>
    /// <param name="color">Color.</param>
    /// <returns></returns>
    public new static RGBA FromWinColor(Color color) =>
        new(color.R, color.G, color.B, color.A);

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("RGBA");
        writer.WriteAttributeString("Red", red.ToString());
        writer.WriteAttributeString("Green", green.ToString());
        writer.WriteAttributeString("Blue", blue.ToString());
        writer.WriteAttributeString("Alpha", alpha.ToString());
        writer.WriteEndElement();
    }

    #endregion
}