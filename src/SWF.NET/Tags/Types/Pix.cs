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
///     Abstract Pix class.
/// </summary>
public abstract class Pix : ISwfSerializer
{
	/// <summary>
	///     Gets the color of the pixel.
	/// </summary>
	public abstract Color PixelColor { get; }

	/// <summary>
	///     Serializes the specified writer.
	/// </summary>
	/// <param name="writer">Writer.</param>
	public abstract void Serialize(XmlWriter writer);
}

/// <summary>
///     Pix15
/// </summary>
public class Pix15 : Pix
{
    #region Properties

    /// <summary>
    ///     Gets the color of the pixel.
    /// </summary>
    public override Color PixelColor =>
        Color.FromArgb(red, green, blue);

    #endregion

    #region Members

    private byte red;
    private byte green;
    private byte blue;

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="Pix15" /> instance.
    /// </summary>
    public Pix15()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="Pix15" /> instance.
    /// </summary>
    /// <param name="red">Red.</param>
    /// <param name="green">Green.</param>
    /// <param name="blue">Blue.</param>
    public Pix15(byte red, byte green, byte blue)
    {
        this.red = red;
        this.green = green;
        this.blue = blue;
    }

    /// <summary>
    ///     Creates a new <see cref="Pix15" /> instance.
    /// </summary>
    /// <param name="bytes">Bytes.</param>
    public Pix15(byte[] bytes)
    {
        var ba = BitParser.GetBitValues(bytes);
        red = (byte)BitParser.ReadUInt32(ba, 1, 5);
        green = (byte)BitParser.ReadUInt32(ba, 6, 5);
        blue = (byte)BitParser.ReadUInt32(ba, 11, 5);
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>size of this structure</returns>
    public static int GetSizeOf() =>
        2;

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BufferedBinaryWriter writer)
    {
        writer.WriteUBits(0, 1);
        writer.WriteUBits((uint)red, 5);
        writer.WriteUBits((uint)green, 5);
        writer.WriteUBits((uint)blue, 5);
    }

    /// <summary>
    ///     Reads the data from a binary reader.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public void ReadData(BufferedBinaryReader binaryReader)
    {
        binaryReader.ReadBoolean();
        red = (byte)binaryReader.ReadUBits(5);
        green = (byte)binaryReader.ReadUBits(5);
        blue = (byte)binaryReader.ReadUBits(5);
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("Pix15");
        writer.WriteAttributeString("Red", red.ToString());
        writer.WriteAttributeString("Green", green.ToString());
        writer.WriteAttributeString("Blue", blue.ToString());
        writer.WriteEndElement();
    }

    #endregion
}

/// <summary>
///     Pix24
/// </summary>
public class Pix24 : Pix
{
    #region Properties

    /// <summary>
    ///     Gets the color of the pixel.
    /// </summary>
    public override Color PixelColor =>
        Color.FromArgb(
            pix24Red,
            pix24Green,
            pix24Blue);

    #endregion

    #region Members

    private byte pix24Red;
    private byte pix24Green;
    private byte pix24Blue;

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="Pix24" /> instance.
    /// </summary>
    public Pix24()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="Pix24" /> instance.
    /// </summary>
    /// <param name="pix24Red">Pix24 red.</param>
    /// <param name="pix24Green">Pix24 green.</param>
    /// <param name="pix24Blue">Pix24 blue.</param>
    public Pix24(byte pix24Red, byte pix24Green, byte pix24Blue)
    {
        this.pix24Red = pix24Red;
        this.pix24Green = pix24Green;
        this.pix24Blue = pix24Blue;
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>size of this structure</returns>
    public static int GetSizeOf() =>
        4;

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BinaryWriter writer)
    {
        writer.Write((byte)0);
        writer.Write(pix24Red);
        writer.Write(pix24Green);
        writer.Write(pix24Blue);
    }

    /// <summary>
    ///     Reads the data from a binary reader.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public void ReadData(BufferedBinaryReader binaryReader)
    {
        var pix24Reserved = binaryReader.ReadByte();
        pix24Red = binaryReader.ReadByte();
        pix24Green = binaryReader.ReadByte();
        pix24Blue = binaryReader.ReadByte();
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("Pix24");
        writer.WriteAttributeString("Red", pix24Red.ToString());
        writer.WriteAttributeString("Green", pix24Green.ToString());
        writer.WriteAttributeString("Blue", pix24Blue.ToString());
        writer.WriteEndElement();
    }

    #endregion
}