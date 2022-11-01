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
///     The CXForm is used to change the colour of a shape or button without
///     changing the values in the original definition of the object.
/// </summary>
/// <remarks>
///     <p>
///         Two types of transformation are supported: <b>Add</b> and <b>Multiply</b>
///     </p>
///     <p>
///         In Add transformations a value is added to each colour channel:
///         <code lang="C#">
/// newRed = red + addRedTerm
/// newGreen = green + addGreenTerm
/// newBlue = blue + addBlueTerm
/// newAlpha = alpha + addAlphaTerm
/// </code>
///     </p>
///     <p>
///         In Multiply transformations each colour channel is multiplied by
///         a given value:
///         <code lang="C#">
/// newRed = red * multiplyRedTerm
/// newGreen = green * multiplyGreenTerm
/// newBlue = blue * multiplyBlueTerm
/// newAlpha = alpha * multiplyAlphaTerm
/// </code>
///     </p>
///     <p>
///         The CXForm was introduced in Flash 1.
///     </p>
/// </remarks>
public class CXForm : ISwfSerializer
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="CXForm" /> instance.
    /// </summary>
    public CXForm()
    {
    }

    #endregion

    #region Members

    private bool hasAddTerms;
    private bool hasMultTerms;

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the red mult terms.
    /// </summary>
    public int RedMultTerms { get; set; } = 0;

    /// <summary>
    ///     Gets or sets the green mult terms.
    /// </summary>
    public int GreenMultTerms { get; set; } = 0;

    /// <summary>
    ///     Gets or sets the blue mult terms.
    /// </summary>
    public int BlueMultTerms { get; set; } = 0;

    /// <summary>
    ///     Gets or sets the red add terms.
    /// </summary>
    public int RedAddTerms { get; set; } = 0;

    /// <summary>
    ///     Gets or sets the green add terms.
    /// </summary>
    public int GreenAddTerms { get; set; } = 0;

    /// <summary>
    ///     Gets or sets the blue add terms.
    /// </summary>
    public int BlueAddTerms { get; set; } = 0;

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    public void ReadData(BufferedBinaryReader binaryReader)
    {
        hasAddTerms = binaryReader.ReadBoolean();
        hasMultTerms = binaryReader.ReadBoolean();
        var nBits = binaryReader.ReadUBits(4);

        if (hasMultTerms)
        {
            var redMultTerms = binaryReader.ReadSBits(nBits);
            var greenMultTerms = binaryReader.ReadSBits(nBits);
            var blueMultTerms = binaryReader.ReadSBits(nBits);
        }

        if (hasAddTerms)
        {
            var redAddTerms = binaryReader.ReadSBits(nBits);
            var greenAddTerms = binaryReader.ReadSBits(nBits);
            var blueAddTerms = binaryReader.ReadSBits(nBits);
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
        if (hasMultTerms)
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
        }

        if (hasAddTerms)
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
        }

        return max;
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf()
    {
        uint res = 6;
        var nBits = GetNumBits();
        if (hasMultTerms)
            res += nBits * 3;
        if (hasAddTerms)
            res += nBits * 3;
        return (int)Math.Ceiling(res / 8.0);
    }

    /// <summary>
    ///     Writes to.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void WriteTo(BufferedBinaryWriter writer)
    {
        writer.SynchBits();
        writer.WriteBoolean(hasAddTerms);
        writer.WriteBoolean(hasMultTerms);
        var nBits = GetNumBits();
        writer.WriteUBits(nBits, 4);

        if (hasMultTerms)
        {
            writer.WriteSBits(RedMultTerms, nBits);
            writer.WriteSBits(GreenMultTerms, nBits);
            writer.WriteSBits(BlueMultTerms, nBits);
        }

        if (hasAddTerms)
        {
            writer.WriteSBits(RedAddTerms, nBits);
            writer.WriteSBits(GreenAddTerms, nBits);
            writer.WriteSBits(BlueAddTerms, nBits);
        }

        writer.SynchBits();
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("CXForm");
        if (hasMultTerms)
        {
            writer.WriteElementString("RedMultTerms", RedMultTerms.ToString());
            writer.WriteElementString("GreenMultTerms", GreenMultTerms.ToString());
            writer.WriteElementString("BlueMultTerms", BlueMultTerms.ToString());
        }

        if (hasAddTerms)
        {
            writer.WriteElementString("RedAddTerms", RedAddTerms.ToString());
            writer.WriteElementString("GreenAddTerms", GreenAddTerms.ToString());
            writer.WriteElementString("BlueAddTerms", BlueAddTerms.ToString());
        }

        writer.WriteEndElement();
    }

    #endregion
}