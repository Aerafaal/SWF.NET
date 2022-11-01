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

using System.Xml;
using SWF.NET.Exceptions;
using SWF.NET.Tags;
using SWF.NET.Tags.Types;
using SWF.NET.Utils;

namespace SWF.NET;

/// <summary>
///     Swf file header object contains main informations
///     about the animation.
/// </summary>
/// <remarks>
///     <p>
///         The header contains those informations:
///         <ul>
///             <li>File signature, to indicate of the file is compressed or not</li>
///             <li>Swf version (1 to 11)</li>
///             <li>Lenght of entire file in bytes</li>
///             <li>Frame size in twips</li>
///             <li>Frame delay in 8.8 fixed number of frame per second</li>
///             <li>Total number of frames in the movie</li>
///         </ul>
///     </p>
/// </remarks>
public class SwfHeader : ISwfSerializer
{
    #region Const Members

    /// <summary>
    ///     Maximum swf version supported
    /// </summary>
    public static int MAX_VERSION = 11;

    #endregion

    #region Members

    /// <summary>
    ///     Signature property ('FWS' or 'CWS').
    /// </summary>
    public string signature;

    /// <summary>
    ///     Private Version.
    /// </summary>
    private byte version;

    /// <summary>
    ///     Private FileSize.
    /// </summary>
    private uint fileSize;

    /// <summary>
    ///     Private rect containing swf dimensions.
    /// </summary>
    private Rect rect;

    /// <summary>
    ///     Private frames per second.
    /// </summary>
    private float fps;

    /// <summary>
    ///     Private total number of frames.
    /// </summary>
    private ushort frames;

    #endregion

    #region Ctor

    /// <summary>
    ///     Constructor.
    /// </summary>
    public SwfHeader()
    {
        fps = 12.0f;
        frames = 0;
        rect = new Rect(0, 0, 550 * 20, 400 * 20);
        version = (byte)MAX_VERSION;
        signature = "FWS";
    }

    /// <summary>
    ///     Creates a new <see cref="SwfHeader" /> instance.
    /// </summary>
    /// <param name="signature">Signature.</param>
    /// <param name="version">Version.</param>
    /// <param name="fileSize">Size of the file.</param>
    /// <param name="dimensions">Dimensions.</param>
    /// <param name="fps">FPS.</param>
    /// <param name="frames">Frames.</param>
    public SwfHeader(
        string signature,
        byte version,
        uint fileSize,
        Rect dimensions,
        float fps,
        ushort frames)
    {
        this.signature = signature;
        Version = version;
        this.fileSize = fileSize;
        rect = dimensions;
        this.fps = fps;
        this.frames = frames;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Get signature as string.
    ///     "FWS" for not compress files.
    ///     "CWS" for compressed file.
    /// </summary>
    public string Signature
    {
        get => signature;
        set => signature = value;
    }

    /// <summary>
    ///     Gets or sets the frames count.
    /// </summary>
    public ushort Frames
    {
        get => frames;
        set => frames = value;
    }

    /// <summary>
    ///     Gets or sets the FPS (frames per second)
    /// </summary>
    public float Fps
    {
        get => fps;
        set => fps = value;
    }

    /// <summary>
    ///     Gets or sets the swf dimensions bound.
    /// </summary>
    public Rect Size
    {
        get => rect;
        set => rect = value;
    }

    /// <summary>
    ///     Gets the width.
    /// </summary>
    public int Width
    {
        get
        {
            if (rect == null)
                return 0;
            return rect.Rectangle.Width;
        }
    }

    /// <summary>
    ///     Gets the height.
    /// </summary>
    public int Height
    {
        get
        {
            if (rect == null)
                return 0;
            return rect.Rectangle.Height;
        }
    }

    /// <summary>
    ///     Gets or sets the size of the file.
    /// </summary>
    public uint FileSize
    {
        get => fileSize;
        set => fileSize = value;
    }

    /// <summary>
    ///     Gets or sets the version.
    /// </summary>
    public byte Version
    {
        get => version;
        set
        {
            version = value;
            if (version > MAX_VERSION)
                throw new InvalidSwfVersionException();
        }
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data from a binary file
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public void ReadData(BufferedBinaryReader binaryReader)
    {
        signature = binaryReader.ReadString(3);
        version = binaryReader.ReadByte();

        if (version > MAX_VERSION)
            throw new InvalidSwfVersionException(version, MAX_VERSION);

        fileSize = binaryReader.ReadUInt32();
        rect = new Rect();
        rect.ReadData(binaryReader);
        binaryReader.SynchBits();
        fps = binaryReader.ReadFloatWord(8, 8);
        frames = binaryReader.ReadUInt16();
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("SwfHeader");
        writer.WriteElementString("Signature", signature);
        writer.WriteElementString("Version", version.ToString());
        rect.Serialize(writer);
        writer.WriteElementString("Fps", fps.ToString());
        writer.WriteElementString("Frames", frames.ToString());
        writer.WriteEndElement();
    }

    #endregion
}