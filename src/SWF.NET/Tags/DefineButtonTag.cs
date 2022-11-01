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
using SWF.NET.Tags.Types;
using SWF.NET.Utils;

namespace SWF.NET.Tags;

/// <summary>
///     DefineButtonTag defines the appearance of a button and
///     the actions performed when the button is clicked.
/// </summary>
/// <remarks>
///     <p>
///         A DefineButtonTag object must contain at least one ButtonRecord object.
///         If more than one button record is defined for a given button
///         state then each shape will be displayed by the button.
///         The order in which the shapes are displayed is determined by
///         the layer assigned to each ButtonRecord object.
///     </p>
///     <p>
///         This tag was introduced in Flash 1.
///     </p>
/// </remarks>
public class DefineButtonTag : BaseTag, DefineTag
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="DefineButtonTag" /> instance.
    /// </summary>
    public DefineButtonTag() =>
        _tagCode = (int)TagCodeEnum.DefineButton;

    /// <summary>
    ///     Creates a new <see cref="DefineButtonTag" /> instance.
    /// </summary>
    /// <param name="buttonId">Button id.</param>
    /// <param name="characters">Characters.</param>
    /// <param name="actions">Actions.</param>
    public DefineButtonTag(ushort buttonId, ButtonRecordCollection characters, byte[] actions)
    {
        this.CharacterId = buttonId;
        this.Characters = characters;
        this.ActionsByteCode = actions;
        _tagCode = (int)TagCodeEnum.DefineButton;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     see <see cref="DefineTag" />
    /// </summary>
    public ushort CharacterId { get; set; }

    /// <summary>
    ///     Gets or sets the characters.
    /// </summary>
    public ButtonRecordCollection Characters { get; set; }

    /// <summary>
    ///     Gets or sets the actions byte code.
    /// </summary>
    public byte[] ActionsByteCode { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void ReadData(byte version, BufferedBinaryReader binaryReader)
    {
        var rh = new RecordHeader();
        rh.ReadData(binaryReader);

        CharacterId = binaryReader.ReadUInt16();
        Characters = new ButtonRecordCollection();

        var characterEndFlag = false;
        while (!characterEndFlag)
        {
            var first = binaryReader.ReadByte();
            if (first == 0)
                characterEndFlag = true;
            else
            {
                var buttRecord = new ButtonRecord();
                buttRecord.ReadData(binaryReader, first, TagCodeEnum.DefineButton);
                Characters.Add(buttRecord);
            }
        }

        var offset = 2;
        foreach (ButtonRecord butRec in Characters)
            offset += butRec.GetSizeOf();

        var lenght = Convert.ToInt32(rh.TagLength) - offset - 1;
        //-1 for the ActionEndFlag
        ActionsByteCode = binaryReader.ReadBytes(lenght);
        //Read ActionEndFlag
        binaryReader.ReadByte();
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>Size of this object</returns>
    protected int GetSizeOf()
    {
        var res = 4;
        if (Characters != null)
            foreach (ButtonRecord buttRec in Characters)
                res += buttRec.GetSizeOf();
        if (ActionsByteCode != null)
            res += ActionsByteCode.Length;

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
        w.Write(CharacterId);
        if (Characters != null)
            foreach (ButtonRecord buttRec in Characters)
                buttRec.WriteTo(w, TagCodeEnum.DefineButton);
        byte end = 0;
        w.Write(end);
        if (ActionsByteCode != null)
            w.Write(ActionsByteCode);
        w.Write(end);

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
        writer.WriteStartElement("DefineButtonTag");
        writer.WriteElementString("ButtonId", CharacterId.ToString());
        if (Characters != null)
            foreach (ButtonRecord buttRec in Characters)
                buttRec.Serialize(writer);
        writer.WriteEndElement();
    }

    #endregion
}