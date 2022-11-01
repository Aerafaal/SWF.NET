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
	�
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

namespace SWF.NET.Tags.Types;

/// <summary>
///     Abstract Video Packet class
/// </summary>
public abstract class VideoPacket
{
    #region Abstract methods

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>The size</returns>
    public abstract int GetSizeOf();

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public abstract void WriteTo(BinaryWriter writer);

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public abstract void ReadData(BufferedBinaryReader binaryReader);

    /// <summary>
    ///     Serializes to the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public abstract void Serialize(XmlWriter writer);

    #endregion
}

/// <summary>
///     H263VideoPacket class
/// </summary>
public class H263VideoPacket : VideoPacket
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="H263VideoPacket" /> instance.
    /// </summary>
    public H263VideoPacket()
    {
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public override int GetSizeOf() =>
        //TODO
        0;

    /// <summary>
    ///     Writes to.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void WriteTo(BinaryWriter writer)
    {
        //TODO
    }

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public override void ReadData(BufferedBinaryReader binaryReader)
    {
        var pictureStartCode = binaryReader.ReadUBits(17);
        var version = binaryReader.ReadUBits(5);
        var temporalRef = binaryReader.ReadUBits(8);

        //TODO...
    }

    /// <summary>
    ///     Serializes to the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void Serialize(XmlWriter writer)
    {
        //TODO
    }

    #endregion
}

/// <summary>
///     ScreenVideoPacket class
/// </summary>
public class ScreenVideoPacket : VideoPacket
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="ScreenVideoPacket" /> instance.
    /// </summary>
    public ScreenVideoPacket()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="ScreenVideoPacket" /> instance.
    /// </summary>
    /// <param name="blockWidth">Width of the block.</param>
    /// <param name="imageWidth">Width of the image.</param>
    /// <param name="blockHeight">Height of the block.</param>
    /// <param name="imageHeight">Height of the image.</param>
    /// <param name="blocks">Blocks.</param>
    public ScreenVideoPacket(
        int blockWidth,
        int imageWidth,
        int blockHeight,
        int imageHeight,
        ImageBlock[] blocks)
    {
        BlockWidth = blockWidth;
        BlockHeight = blockHeight;
        ImageWidth = imageWidth;
        ImageHeight = imageHeight;
        Blocks = blocks;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the blocks.
    /// </summary>
    public ImageBlock[] Blocks { get; set; }

    /// <summary>
    ///     Gets or sets the height of the image.
    /// </summary>
    public int ImageHeight { get; set; }

    /// <summary>
    ///     Gets or sets the width of the image.
    /// </summary>
    public int ImageWidth { get; set; }

    /// <summary>
    ///     Gets or sets the height of the block.
    /// </summary>
    public int BlockHeight { get; set; }

    /// <summary>
    ///     Gets or sets the width of the block.
    /// </summary>
    public int BlockWidth { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public override int GetSizeOf() =>
        //TODO
        0;

    /// <summary>
    ///     Writes to.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void WriteTo(BinaryWriter writer)
    {
        //TODO
    }

    /// <summary>
    ///     Reads the data from the binary reader.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public override void ReadData(BufferedBinaryReader binaryReader)
    {
        var b = binaryReader.ReadBytes(4);
        var ba = BitParser.GetBitValues(b);

        BlockWidth = BitParser.ReadUInt32(ba, 0, 4);
        ImageWidth = BitParser.ReadUInt32(ba, 4, 12);
        BlockHeight = BitParser.ReadUInt32(ba, 16, 4);
        ImageHeight = BitParser.ReadUInt32(ba, 20, 12);

        var nbWBlock = 0;
        var nbBlockWInt = ImageWidth / BlockWidth;
        float nbBlockWDec = ImageWidth / BlockWidth;
        if (nbBlockWInt == nbBlockWDec)
            nbWBlock = nbBlockWInt;
        else
            nbWBlock = nbBlockWInt + 1;

        var nbHBlock = 0;
        var nbBlockHInt = ImageHeight / BlockHeight;
        float nbBlockHDec = ImageHeight / BlockHeight;
        if (nbBlockHInt == nbBlockHDec)
            nbHBlock = nbBlockHInt;
        else
            nbHBlock = nbBlockHInt + 1;

        var nbBlock = nbWBlock * nbHBlock;

        if (nbBlock > 0)
        {
            Blocks = new ImageBlock[nbBlock];

            for (var i = 0; i < nbBlock; i++)
            {
                Blocks[i] = new ImageBlock();
                Blocks[i].ReadData(binaryReader);
            }
        }
    }

    /// <summary>
    ///     Serializes to the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void Serialize(XmlWriter writer)
    {
        //TODO
    }

    #endregion
}

/// <summary>
///     Video ImageBlock class
/// </summary>
public class ImageBlock
{
    #region Properties

    /// <summary>
    ///     Gets or sets the data.
    /// </summary>
    public byte[] Data { get; set; }

    #endregion

    #region Members

    private int dataSize;

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="ImageBlock" /> instance.
    /// </summary>
    public ImageBlock()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="ImageBlock" /> instance.
    /// </summary>
    /// <param name="data">Data.</param>
    public ImageBlock(byte[] data)
    {
        Data = data;
        dataSize = data.Length;
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>size of the structure</returns>
    public int GetSizeOf()
    {
        var res = 2;
        if (Data != null)
            res += Data.Length;
        return res;
    }

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BinaryWriter writer)
    {
        //TODO
    }

    /// <summary>
    ///     Reads the data from a binary reader.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public void ReadData(BufferedBinaryReader binaryReader)
    {
        var b = binaryReader.ReadBytes(2);
        var ba = BitParser.GetBitValues(b);

        dataSize = BitParser.ReadUInt32(ba, 0, 16);

        Data = null;
        if (dataSize != 0)
        {
            Data = new byte[dataSize];
            for (var i = 0; i < dataSize; i++)
                Data[i] = binaryReader.ReadByte();
        }
    }

    #endregion
}