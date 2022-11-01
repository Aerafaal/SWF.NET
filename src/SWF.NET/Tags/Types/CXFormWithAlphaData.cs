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
using System.Xml;
using SWF.NET.Utils;

namespace SWF.NET.Tags.Types;

/// <summary>
///     CXFormWithAlphaData.
/// </summary>
public class CXFormWithAlphaData : ISwfSerializer
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="CXFormWithAlphaData" /> instance.
    /// </summary>
    public CXFormWithAlphaData()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="CXFormWithAlphaData" /> instance.
    /// </summary>
    /// <param name="redMultTerms">Red mult terms.</param>
    /// <param name="greenMultTerms">Green mult terms.</param>
    /// <param name="blueMultTerms">Blue mult terms.</param>
    /// <param name="alphaMultTerms">Alpha mult terms.</param>
    public CXFormWithAlphaData(int redMultTerms, int greenMultTerms, int blueMultTerms, int alphaMultTerms)
    {
        this.RedMultTerms = redMultTerms;
        this.GreenMultTerms = greenMultTerms;
        this.BlueMultTerms = blueMultTerms;
        this.AlphaMultTerms = alphaMultTerms;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the red add terms.
    /// </summary>
    /// <value></value>
    public int RedAddTerms { get; set; }

    /// <summary>
    ///     Gets or sets the green add terms.
    /// </summary>
    /// <value></value>
    public int GreenAddTerms { get; set; }

    /// <summary>
    ///     Gets or sets the blue add terms.
    /// </summary>
    /// <value></value>
    public int BlueAddTerms { get; set; }

    /// <summary>
    ///     Gets or sets the alpha add terms.
    /// </summary>
    /// <value></value>
    public int AlphaAddTerms { get; set; }


    /// <summary>
    ///     Gets or sets the red mult terms.
    /// </summary>
    /// <value></value>
    public int RedMultTerms { get; set; }

    /// <summary>
    ///     Gets or sets the green mult terms.
    /// </summary>
    /// <value></value>
    public int GreenMultTerms { get; set; }

    /// <summary>
    ///     Gets or sets the blue mult terms.
    /// </summary>
    /// <value></value>
    public int BlueMultTerms { get; set; }

    /// <summary>
    ///     Gets or sets the alpha mult terms.
    /// </summary>
    /// <value></value>
    public int AlphaMultTerms { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Gets a value indicating whether this instance has add terms.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance has add terms; otherwise, <c>false</c>.
    /// </value>
    private bool HasAddTerms =>
        RedAddTerms != 0 ||
        GreenAddTerms != 0 ||
        BlueAddTerms != 0 ||
        AlphaAddTerms != 0;

    /// <summary>
    ///     Gets a value indicating whether this instance has mult terms.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance has mult terms; otherwise, <c>false</c>.
    /// </value>
    private bool HasMultTerms =>
        RedMultTerms != 0 ||
        GreenMultTerms != 0 ||
        BlueMultTerms != 0 ||
        AlphaMultTerms != 0;

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public void ReadData(BufferedBinaryReader binaryReader)
    {
        var hasAddTerms = binaryReader.ReadBoolean();
        var hasMultTerms = binaryReader.ReadBoolean();
        var nBits = binaryReader.ReadUBits(4);

        if (hasMultTerms)
        {
            RedMultTerms = binaryReader.ReadSBits(nBits);
            GreenMultTerms = binaryReader.ReadSBits(nBits);
            BlueMultTerms = binaryReader.ReadSBits(nBits);
            AlphaMultTerms = binaryReader.ReadSBits(nBits);
        }

        if (hasAddTerms)
        {
            RedAddTerms = binaryReader.ReadSBits(nBits);
            GreenAddTerms = binaryReader.ReadSBits(nBits);
            BlueAddTerms = binaryReader.ReadSBits(nBits);
            AlphaAddTerms = binaryReader.ReadSBits(nBits);
        }
    }

    /// <summary>
    ///     Gets the num bits.
    /// </summary>
    /// <returns></returns>
    private uint GetNumBits()
    {
        uint max = 0;
        uint tmp = 0;
        if (HasMultTerms)
        {
            tmp = BufferedBinaryWriter.GetNumBits(RedMultTerms);
            if (tmp > max)
                max = tmp;
            tmp = BufferedBinaryWriter.GetNumBits(GreenMultTerms);
            if (tmp > max)
                max = tmp;
            tmp = BufferedBinaryWriter.GetNumBits(BlueMultTerms);
            if (tmp > max)
                max = tmp;
            tmp = BufferedBinaryWriter.GetNumBits(AlphaMultTerms);
            if (tmp > max)
                max = tmp;
        }

        if (HasAddTerms)
        {
            tmp = BufferedBinaryWriter.GetNumBits(RedAddTerms);
            if (tmp > max)
                max = tmp;
            tmp = BufferedBinaryWriter.GetNumBits(GreenAddTerms);
            if (tmp > max)
                max = tmp;
            tmp = BufferedBinaryWriter.GetNumBits(BlueAddTerms);
            if (tmp > max)
                max = tmp;
            tmp = BufferedBinaryWriter.GetNumBits(AlphaAddTerms);
            if (tmp > max)
                max = tmp;
        }

        return max;
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf()
    {
        var res = 6;
        var nBits = GetNumBits();
        if (HasMultTerms)
            res += (int)(nBits * 4);
        if (HasAddTerms)
            res += (int)(nBits * 4);
        return (int)Math.Ceiling(res / 8.0);
    }

    /// <summary>
    ///     Writes to.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BufferedBinaryWriter writer)
    {
        writer.WriteBoolean(HasAddTerms);
        writer.WriteBoolean(HasMultTerms);
        var nBits = GetNumBits();
        writer.WriteUBits(nBits, 4);

        if (HasMultTerms)
        {
            writer.WriteSBits(RedMultTerms, nBits);
            writer.WriteSBits(GreenMultTerms, nBits);
            writer.WriteSBits(BlueMultTerms, nBits);
            writer.WriteSBits(AlphaMultTerms, nBits);
        }

        if (HasAddTerms)
        {
            writer.WriteSBits(RedAddTerms, nBits);
            writer.WriteSBits(GreenAddTerms, nBits);
            writer.WriteSBits(BlueAddTerms, nBits);
            writer.WriteSBits(AlphaAddTerms, nBits);
        }
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("CXFormWithAlphaData");
        writer.WriteAttributeString("HasMultTerms", HasMultTerms.ToString());
        writer.WriteAttributeString("HasAddTerms", HasAddTerms.ToString());
        writer.WriteAttributeString("Nbits", GetNumBits().ToString());
        if (HasMultTerms)
        {
            writer.WriteElementString("RedMultTerms", RedMultTerms.ToString());
            writer.WriteElementString("GreenMultTerms", GreenMultTerms.ToString());
            writer.WriteElementString("BlueMultTerms", BlueMultTerms.ToString());
            writer.WriteElementString("AlphaMultTerms", AlphaMultTerms.ToString());
        }

        if (HasAddTerms)
        {
            writer.WriteElementString("RedAddTerms", RedAddTerms.ToString());
            writer.WriteElementString("GreenAddTerms", GreenAddTerms.ToString());
            writer.WriteElementString("BlueAddTerms", BlueAddTerms.ToString());
            writer.WriteElementString("AlphaAddTerms", AlphaAddTerms.ToString());
        }

        writer.WriteEndElement();
    }

    #endregion
}