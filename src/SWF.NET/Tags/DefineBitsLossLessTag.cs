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

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Xml;
using SWF.NET.Exceptions;
using SWF.NET.Tags.Types;
using SWF.NET.Utils;
using SWF.NET.ZIP.Zip.Compression;
using SWF.NET.ZIP.Zip.Compression.Streams;

namespace SWF.NET.Tags;

/// <summary>
///     DefineBitsLossLessTag  is used to define an image compressed using the
///     lossless zlib compression algorithm.
/// </summary>
/// <remarks>
///     <p>
///         The class supports color-mapped images where the image data contains
///         an index into a color table or images where the image data specifies
///         the color directly.
///     </p>
///     <p>
///         For color-mapped images the color table contains up to 256, 24-bit colors.
///         The image contains one byte for each pixel which is an index into the table
///         to specify the color for that pixel. The color table and the image data
///         are compressed as a single block, with the color table placed before the image.
///     </p>
///     <p>
///         For images where the color is specified directly, the image data contains
///         either 16 or 24 bit color values. For 16-bit color values the most significant
///         bit is zero followed by three, 5-bit fields for the red, green and blue channels.
///     </p>
///     <p>
///         Four bytes are used to represent 24-bit colors. The first byte is always set to
///         zero and the following bytes contain the color values for the red, green and
///         blue color channels.
///     </p>
///     <p>
///         The number of bytes in each row of an image must be aligned to a 32-bit word
///         boundary. For example if an image if an icon is 25 pixels wide, then for an 8-bit
///         color mapped image an additional three bytes (0x00) must be used to pad each row;
///         for a 16-bit direct mapped color image an additional two bytes must be used as
///         padding.
///     </p>
///     <p>
///         The image data is stored in zlib compressed form within the object. For
///         color-mapped images the compressed data contains the color table followed by the
///         image data. The color table is omitted for direct-mapped images.
///     </p>
///     <p>
///         This tag was introduced in Flash 2.
///     </p>
/// </remarks>
public class DefineBitsLossLessTag : BaseTag, DefineTag
{
    #region Members

    private byte _bitmapColorTableSize;

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="DefineBitsLossLessTag" /> instance.
    /// </summary>
    public DefineBitsLossLessTag() =>
        _tagCode = (int)TagCodeEnum.DefineBitsLossLess;

    /// <summary>
    ///     constructor.
    /// </summary>
    /// <param name="characterId">id for this character</param>
    /// <param name="bitmapFormat">Format of compressed data</param>
    /// <param name="bitmapWidth">Width of bitmap image</param>
    /// <param name="bitmapHeight">Height of bitmap image</param>
    /// <param name="bitmapColorTableSize">actual number of colors in the color table</param>
    public DefineBitsLossLessTag(
        ushort characterId,
        byte bitmapFormat,
        ushort bitmapWidth,
        ushort bitmapHeight,
        byte bitmapColorTableSize)
    {
        CharacterId = characterId;
        BitmapFormat = bitmapFormat;
        BitmapWidth = bitmapWidth;
        BitmapHeight = bitmapHeight;
        _bitmapColorTableSize = bitmapColorTableSize;
        _tagCode = (int)TagCodeEnum.DefineBitsLossLess;
    }

    /// <summary>
    ///     constructor.
    /// </summary>
    /// <param name="characterId">id for this character</param>
    /// <param name="bitmapFormat">Format of compressed data</param>
    /// <param name="bitmapWidth">Width of bitmap image</param>
    /// <param name="bitmapHeight">Height of bitmap image</param>
    /// <param name="bitmapColorTableSize">actual number of colors in the color table</param>
    /// <param name="zlibBitmapData">zlib compressed bitmap data</param>
    public DefineBitsLossLessTag(
        ushort characterId,
        byte bitmapFormat,
        ushort bitmapWidth,
        ushort bitmapHeight,
        byte bitmapColorTableSize,
        ColorMapData zlibBitmapData)
    {
        CharacterId = characterId;
        BitmapFormat = bitmapFormat;
        BitmapWidth = bitmapWidth;
        BitmapHeight = bitmapHeight;
        _bitmapColorTableSize = bitmapColorTableSize;
        ColorMapData = zlibBitmapData;
        _tagCode = (int)TagCodeEnum.DefineBitsLossLess;
    }

    /// <summary>
    ///     constructor.
    /// </summary>
    /// <param name="characterId">id for this character</param>
    /// <param name="bitmapFormat">Format of compressed data</param>
    /// <param name="bitmapWidth">Width of bitmap image</param>
    /// <param name="bitmapHeight">Height of bitmap image</param>
    /// <param name="bitmapColorTableSize">actual number of colors in the color table</param>
    /// <param name="zlibBitmapData">zlib compressed bitmap data</param>
    public DefineBitsLossLessTag(
        ushort characterId,
        byte bitmapFormat,
        ushort bitmapWidth,
        ushort bitmapHeight,
        byte bitmapColorTableSize,
        BitmapColorData zlibBitmapData)
    {
        CharacterId = characterId;
        BitmapFormat = bitmapFormat;
        BitmapWidth = bitmapWidth;
        BitmapHeight = bitmapHeight;
        _bitmapColorTableSize = bitmapColorTableSize;
        BitmapData = zlibBitmapData;
        _tagCode = (int)TagCodeEnum.DefineBitsLossLess;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the bitmap format.
    /// </summary>
    public byte BitmapFormat { get; set; }

    /// <summary>
    ///     Gets or sets the color map data.
    /// </summary>
    public ColorMapData ColorMapData { get; set; }

    /// <summary>
    ///     Gets or sets the bitmap data.
    /// </summary>
    public BitmapColorData BitmapData { get; set; }

    /// <summary>
    ///     Gets or sets the width of the bitmap.
    /// </summary>
    public ushort BitmapWidth { get; set; }

    /// <summary>
    ///     Gets or sets the height of the bitmap.
    /// </summary>
    public ushort BitmapHeight { get; set; }

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

        var beforePos = (int)binaryReader.BaseStream.Position;
        var toReaded = (int)rh.TagLength - 7;

        CharacterId = binaryReader.ReadUInt16();
        BitmapFormat = binaryReader.ReadByte();
        BitmapWidth = binaryReader.ReadUInt16();
        BitmapHeight = binaryReader.ReadUInt16();
        _bitmapColorTableSize = 0;

        if (BitmapFormat == 3)
        {
            _bitmapColorTableSize = binaryReader.ReadByte();
            toReaded--;
        }

        if (BitmapFormat == 3)
        {
            ColorMapData = new ColorMapData();
            ColorMapData.ReadData(binaryReader, _bitmapColorTableSize, BitmapWidth, BitmapHeight, toReaded);
        }
        else if (BitmapFormat == 4 || BitmapFormat == 5)
        {
            var imageSize = BitmapWidth * BitmapHeight;
            var uncompressedSize = imageSize;
            if (BitmapFormat == 4)
                uncompressedSize *= 2;
            else
                uncompressedSize *= 4;

            var uncompressed = new byte[uncompressedSize];
            var compressed = binaryReader.ReadBytes(toReaded);
            var zipInflator = new Inflater();
            zipInflator.SetInput(compressed);
            zipInflator.Inflate(uncompressed, 0, uncompressedSize);

            BitmapData = null;
            if (BitmapFormat == 4)
            {
                var bitmapPixelData = new Pix15[imageSize];
                for (int i = 0, j = 0; i < imageSize; i++, j += 2)
                {
                    var data = new byte[2] { uncompressed[j], uncompressed[j + 1] };
                    bitmapPixelData[i] = new Pix15(data);
                }

                BitmapData = new BitmapColorData(bitmapPixelData);
            }
            else
            {
                var bitmapPixelData = new Pix24[imageSize];
                for (int i = 0, j = 0; i < imageSize; i++, j += 4)
                {
                    var reserved = uncompressed[j];
                    var red = uncompressed[j + 1];
                    var green = uncompressed[j + 2];
                    var blue = uncompressed[j + 3];
                    bitmapPixelData[i] = new Pix24(red, green, blue);
                }

                BitmapData = new BitmapColorData(bitmapPixelData);
            }
        }
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf(int sizeOfCompressedData)
    {
        var length = 7;
        if (BitmapFormat == 3)
        {
            length++;
            length += sizeOfCompressedData;
        }
        else if (BitmapFormat == 4 || BitmapFormat == 5) length += sizeOfCompressedData;

        return length;
    }

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void UpdateData(byte version)
    {
        if (version < 2)
            return;

        // Compression process
        var lenghtOfCompressedBlock = 0;
        byte[] compressArray = null;
        var unCompressedStream = new MemoryStream();
        var unCompressedWriter = new BufferedBinaryWriter(unCompressedStream);

        if (BitmapFormat == 3)
            ColorMapData.WriteTo(unCompressedWriter);
        else if (BitmapFormat == 4 || BitmapFormat == 5) BitmapData.WriteTo(unCompressedWriter);

        var compressedStream = new MemoryStream();
        var ouput = new DeflaterOutputStream(compressedStream);
        var unCompressArray = unCompressedStream.ToArray();
        ouput.Write(unCompressArray, 0, unCompressArray.Length);
        ouput.Finish();
        compressArray = compressedStream.ToArray();
        lenghtOfCompressedBlock = compressArray.Length;
        ouput.Close();
        unCompressedStream.Close();

        //Writing process
        var m = new MemoryStream();
        var w = new BufferedBinaryWriter(m);

        var rh = new RecordHeader(TagCode, GetSizeOf(lenghtOfCompressedBlock));

        rh.WriteTo(w);
        w.Write(CharacterId);
        w.Write(BitmapFormat);
        w.Write(BitmapWidth);
        w.Write(BitmapHeight);

        if (BitmapFormat == 3)
        {
            w.Write(_bitmapColorTableSize);
            w.Write(compressArray);
        }
        else if (BitmapFormat == 4 || BitmapFormat == 5) w.Write(compressArray);

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
        writer.WriteStartElement("DefineBitsLossLessTag");
        writer.WriteAttributeString("CharacterId", CharacterId.ToString());
        writer.WriteElementString("BitmapFormat", BitmapFormat.ToString());
        writer.WriteElementString("BitmapWidth", BitmapWidth.ToString());
        writer.WriteElementString("BitmapHeight", BitmapHeight.ToString());
        writer.WriteEndElement();
    }

    #endregion

    #region Compile & Decompile Methods

    /// <summary>
    ///     Construct a new DefineBitsLossLessTag object
    ///     from a file.
    /// </summary>
    /// <param name="characterId">Character id.</param>
    /// <param name="fileName">Name of the file.</param>
    /// <returns></returns>
    public static DefineBitsLossLessTag FromFile(ushort characterId, string fileName)
    {
        var stream = File.OpenRead(fileName);
        var res = FromImage(characterId, Image.FromStream(stream));
        stream.Close();
        return res;
    }

    /// <summary>
    ///     Construct a new DefineBitsLossLessTag object
    ///     from a stream.
    /// </summary>
    /// <param name="characterId">Character id.</param>
    /// <param name="stream">Stream.</param>
    /// <returns></returns>
    public static DefineBitsLossLessTag FromStream(ushort characterId, Stream stream)
    {
        var res = FromImage(characterId, Image.FromStream(stream));
        return res;
    }

    /// <summary>
    ///     Construct a new DefineBitsLossLessTag object
    ///     from an image object.
    /// </summary>
    /// <param name="characterId">Character id.</param>
    /// <param name="image">Image.</param>
    /// <returns></returns>
    public static DefineBitsLossLessTag FromImage(ushort characterId, Image image)
    {
        if (image.RawFormat.Equals(ImageFormat.Bmp) == false &&
            image.RawFormat.Equals(ImageFormat.MemoryBmp) == false)
            throw new InvalidImageFormatException();

        var bitmap = (Bitmap)image;
        byte format = 0;
        var pxFormat = bitmap.PixelFormat;
        if (pxFormat.Equals(PixelFormat.Format8bppIndexed))
            format = 3;
        else if (pxFormat.Equals(PixelFormat.Format16bppRgb555) ||
                 pxFormat.Equals(PixelFormat.Format16bppRgb565))
            format = 4;
        else if (pxFormat.Equals(PixelFormat.Format24bppRgb))
            format = 5;
        else
            throw new InvalidPixelFormatException();

        var bmp = new DefineBitsLossLessTag();
        bmp.CharacterId = characterId;
        bmp.BitmapFormat = format;
        bmp.BitmapWidth = (ushort)bitmap.Width;
        bmp.BitmapHeight = (ushort)bitmap.Height;

        var imageSize = bitmap.Width * bitmap.Height;

        if (bmp.BitmapFormat == 3)
        {
            //TODO
        }
        else if (bmp.BitmapFormat == 4)
        {
            var bitmapPixelData = new Pix15[imageSize];
            var k = 0;
            for (var i = 0; i < bitmap.Height; i++)
            for (var j = 0; j < bitmap.Width; j++)
            {
                var color = bitmap.GetPixel(j, i);
                bitmapPixelData[k] = new Pix15(color.R, color.G, color.B);
                k++;
            }

            bmp.BitmapData = new BitmapColorData(bitmapPixelData);
        }
        else if (bmp.BitmapFormat == 5)
        {
            var bitmapPixelData = new Pix24[imageSize];
            var k = 0;
            for (var i = 0; i < bitmap.Height; i++)
            for (var j = 0; j < bitmap.Width; j++)
            {
                var color = bitmap.GetPixel(j, i);
                bitmapPixelData[k] = new Pix24(color.R, color.G, color.B);
                k++;
            }

            bmp.BitmapData = new BitmapColorData(bitmapPixelData);
        }

        return bmp;
    }

    /// <summary>
    ///     Decompiles to file.
    /// </summary>
    /// <param name="fileName">Name of the file.</param>
    public void DecompileToFile(string fileName)
    {
        Stream stream = File.OpenWrite(fileName);
        DecompileToStream(stream);
        stream.Close();
    }

    /// <summary>
    ///     Decompiles to image.
    /// </summary>
    /// <returns></returns>
    public Image DecompileToImage()
    {
        var stream = new MemoryStream();
        DecompileToStream(stream);
        return Image.FromStream(stream);
    }

    /// <summary>
    ///     Decompiles to stream.
    /// </summary>
    /// <param name="stream">Stream.</param>
    private void DecompileToStream(Stream stream)
    {
        if (BitmapFormat == 3)
        {
            //ColorMapData
            var mapData = ColorMapData;
            var bmp = new Bitmap(BitmapWidth, BitmapHeight);
            var k = 0;
            for (var i = 0; i < BitmapHeight; i++)
            for (var j = 0; j < BitmapWidth; j++)
            {
                int index = mapData.ColorMapPixelData[k];
                if (index >= 0 && index < mapData.ColorTableRGB.Length)
                    bmp.SetPixel(j, i, mapData.ColorTableRGB[index].ToWinColor());
                else
                    bmp.SetPixel(j, i, Color.Black);
                k++;
            }

            bmp.Save(stream, ImageFormat.Bmp);
            bmp.Dispose();
        }
        else
        {
            //BitmapData
            var bitmapData = BitmapData;
            Pix[] data = null;
            if (BitmapFormat == 5)
                data = bitmapData.bitmapPixelDataPix24;
            else
                data = bitmapData.bitmapPixelDataPix15;

            if (data == null)
                return;

            var bmp = new Bitmap(BitmapWidth, BitmapHeight);
            var k = 0;
            for (var i = 0; i < BitmapHeight; i++)
            for (var j = 0; j < BitmapWidth; j++)
            {
                if (k < data.Length)
                    bmp.SetPixel(j, i, data[k].PixelColor);
                else
                    bmp.SetPixel(j, i, Color.Black);
                k++;
            }

            bmp.Save(stream, ImageFormat.Bmp);
            bmp.Dispose();
        }
    }

    #endregion
}