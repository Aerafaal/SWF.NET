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

using System.IO;
using System.Xml;
using SWF.NET.Utils;
using SWF.NET.ZIP.Zip.Compression;

namespace SWF.NET.Tags.Types;

/// <summary>
///     ColorMapData
/// </summary>
public class ColorMapData : ISwfSerializer
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="ColorMapData" /> instance.
    /// </summary>
    public ColorMapData()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="ColorMapData" /> instance.
    /// </summary>
    /// <param name="colorTableRGB">Color table RGB.</param>
    /// <param name="colorMapPixelData">Color map pixel data.</param>
    public ColorMapData(RGB[] colorTableRGB, byte[] colorMapPixelData)
    {
        ColorTableRGB = colorTableRGB;
        ColorMapPixelData = colorMapPixelData;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the color map pixel data.
    /// </summary>
    public byte[] ColorMapPixelData { get; set; }

    /// <summary>
    ///     Gets or sets the color table RGB.
    /// </summary>
    public RGB[] ColorTableRGB { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="reader">Reader.</param>
    /// <param name="bitmapColorTableSize">Size of the bitmap color table.</param>
    /// <param name="bitmapWidth">Width of the bitmap.</param>
    /// <param name="bitmapHeight">Height of the bitmap.</param>
    /// <param name="toRead">To read.</param>
    public void ReadData(
        BufferedBinaryReader reader,
        byte bitmapColorTableSize,
        ushort bitmapWidth,
        ushort bitmapHeight,
        int toRead)
    {
        var size = (bitmapColorTableSize + 1) * 3 + bitmapWidth * bitmapHeight;
        var uncompressed = new byte[size];

        var compressed = reader.ReadBytes(toRead);
        var zipInflator = new Inflater();
        zipInflator.SetInput(compressed);
        zipInflator.Inflate(uncompressed, 0, size);

        var readed = 0;
        var offset = size;

        ColorTableRGB = new RGB[bitmapColorTableSize + 1];
        for (var i = 0; i < bitmapColorTableSize + 1; i++)
        {
            var red = uncompressed[readed];
            readed++;
            var green = uncompressed[readed];
            readed++;
            var blue = uncompressed[readed];
            readed++;
            ColorTableRGB[i] = new RGB(red, green, blue);
            offset -= 3;
        }

        ColorMapPixelData = new byte[offset];
        for (var i = 0; i < offset; i++, readed++)
            ColorMapPixelData[i] = uncompressed[readed];
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>size of this type</returns>
    public int GetSizeOf()
    {
        var res = 0;
        if (ColorTableRGB != null)
            res += ColorTableRGB.Length * 3;
        if (ColorMapPixelData != null)
            res += ColorMapPixelData.Length * 1;
        return res;
    }

    /// <summary>
    ///     Writes to a binary writer
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BinaryWriter writer)
    {
        if (ColorTableRGB != null)
        {
            var enums = ColorTableRGB.GetEnumerator();
            while (enums.MoveNext())
                ((RGB)enums.Current).WriteTo(writer);
        }

        writer.Write(ColorMapPixelData);
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("ColorMapData");
        writer.WriteEndElement();
    }

    #endregion
}

/// <summary>
///     AlphaColorMapData
/// </summary>
public class AlphaColorMapData : ISwfSerializer
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="AlphaColorMapData" /> instance.
    /// </summary>
    public AlphaColorMapData()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="AlphaColorMapData" /> instance.
    /// </summary>
    /// <param name="colorTableRgb">Color table RGB.</param>
    /// <param name="colorMapPixelData">Color map pixel data.</param>
    public AlphaColorMapData(RGBA[] colorTableRgb, byte[] colorMapPixelData)
    {
        ColorTableRgb = colorTableRgb;
        ColorMapPixelData = colorMapPixelData;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the color table RGBA.
    /// </summary>
    public RGBA[] ColorTableRgb { get; set; }

    /// <summary>
    ///     Gets or sets the color map pixel data.
    /// </summary>
    public byte[] ColorMapPixelData { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>size of this type</returns>
    public int GetSizeOf()
    {
        var res = 0;
        if (ColorTableRgb != null)
            res += ColorTableRgb.Length * 4;
        if (ColorMapPixelData != null)
            res += ColorMapPixelData.Length * 1;
        return res;
    }

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BinaryWriter writer)
    {
        if (ColorTableRgb != null)
        {
            var enums = ColorTableRgb.GetEnumerator();
            while (enums.MoveNext())
                ((RGBA)enums.Current).WriteTo(writer);
        }

        writer.Write(ColorMapPixelData);
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("AlphaColorMapData");
        //foreach (RGBA rgb in colorTableRgb)
        //	rgb.Serialize(writer);
        writer.WriteEndElement();
    }

    #endregion
}

/// <summary>
///     AlphaBitmapData
/// </summary>
public class AlphaBitmapData : ISwfSerializer
{
    #region Members

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the bitmap pixel data.
    /// </summary>
    public RGBA[] BitmapPixelData { get; set; }

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="AlphaBitmapData" /> instance.
    /// </summary>
    public AlphaBitmapData()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="AlphaBitmapData" /> instance.
    /// </summary>
    /// <param name="bitmapPixelData">Bitmap pixel data.</param>
    public AlphaBitmapData(RGBA[] bitmapPixelData) =>
        BitmapPixelData = bitmapPixelData;

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>size of this type</returns>
    public int GetSizeOf()
    {
        var res = 0;
        if (BitmapPixelData != null)
            res += BitmapPixelData.Length * 4;
        return res;
    }

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BinaryWriter writer)
    {
        if (BitmapPixelData != null)
        {
            var enums = BitmapPixelData.GetEnumerator();
            while (enums.MoveNext())
                ((RGBA)enums.Current).WriteTo(writer);
        }
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("AlphaBitmapData");
        /*
        if (bitmapPixelData != null)
        {
            IEnumerator enums = bitmapPixelData.GetEnumerator();
            while(enums.MoveNext())
                ((RGBA)enums.Current).Serialize(writer);
        }
        */
        writer.WriteEndElement();
    }

    #endregion
}