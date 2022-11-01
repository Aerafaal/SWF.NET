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
///     DefineButtonSoundTag defines the sounds that are played when an
///     event occurs in a button.
/// </summary>
/// <remarks>
///     <p>
///         A sound is played for only a subset of the events that a button
///         responds to:
///         <ul>
///             <li>RollOver: The cursor enters the active area of the button.</li>
///             <li>RollOut: The cursor exits the active area of the button.</li>
///             <li>
///                 Press: The mouse button is clicked and the cursor is inside the
///                 active area of the button.
///             </li>
///             <li>
///                 Release: The mouse button is released while the cursor is inside
///                 the active area of the button.
///             </li>
///         </ul>
///     </p>
///     <p>
///         This tag was introduced in Flash 2.
///     </p>
/// </remarks>
public class DefineButtonSoundTag : BaseTag, DefineTag
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="DefineButtonSoundTag" /> instance.
    /// </summary>
    public DefineButtonSoundTag() =>
        _tagCode = (int)TagCodeEnum.DefineButtonSound;

    /// <summary>
    ///     Creates a new <see cref="DefineButtonSoundTag" /> instance.
    /// </summary>
    /// <param name="buttonId">Button id.</param>
    /// <param name="buttonSoundChar">Button sound char.</param>
    /// <param name="buttonSoundInfo">Button sound info.</param>
    /// <param name="buttonSoundChar1">Button sound char1.</param>
    /// <param name="buttonSoundInfo1">Button sound info1.</param>
    /// <param name="buttonSoundChar2">Button sound char2.</param>
    /// <param name="buttonSoundInfo2">Button sound info2.</param>
    /// <param name="buttonSoundChar3">Button sound char3.</param>
    /// <param name="buttonSoundInfo3">Button sound info3.</param>
    public DefineButtonSoundTag(
        ushort buttonId,
        ushort buttonSoundChar,
        SoundInfo buttonSoundInfo,
        ushort buttonSoundChar1,
        SoundInfo buttonSoundInfo1,
        ushort buttonSoundChar2,
        SoundInfo buttonSoundInfo2,
        ushort buttonSoundChar3,
        SoundInfo buttonSoundInfo3)
    {
        this.CharacterId = buttonId;
        this.ButtonSoundChar = buttonSoundChar;
        this.ButtonSoundInfo = buttonSoundInfo;
        this.ButtonSoundChar1 = buttonSoundChar1;
        this.ButtonSoundInfo1 = buttonSoundInfo1;
        this.ButtonSoundChar2 = buttonSoundChar2;
        this.ButtonSoundInfo2 = buttonSoundInfo2;
        this.ButtonSoundChar3 = buttonSoundChar3;
        this.ButtonSoundInfo3 = buttonSoundInfo3;
        _tagCode = (int)TagCodeEnum.DefineButtonSound;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     see <see cref="DefineTag" />
    /// </summary>
    public ushort CharacterId { get; set; }

    /// <summary>
    ///     Gets or sets the button sound char.
    /// </summary>
    public ushort ButtonSoundChar { get; set; }

    /// <summary>
    ///     Gets or sets the button sound info.
    /// </summary>
    public SoundInfo ButtonSoundInfo { get; set; }

    /// <summary>
    ///     Gets or sets the button sound char1.
    /// </summary>
    public ushort ButtonSoundChar1 { get; set; }

    /// <summary>
    ///     Gets or sets the button sound info1.
    /// </summary>
    public SoundInfo ButtonSoundInfo1 { get; set; }

    /// <summary>
    ///     Gets or sets the button sound char2.
    /// </summary>
    public ushort ButtonSoundChar2 { get; set; }

    /// <summary>
    ///     Gets or sets the button sound info2.
    /// </summary>
    public SoundInfo ButtonSoundInfo2 { get; set; }

    /// <summary>
    ///     Gets or sets the button sound char3.
    /// </summary>
    public ushort ButtonSoundChar3 { get; set; }

    /// <summary>
    ///     Gets or sets the button sound info3.
    /// </summary>
    public SoundInfo ButtonSoundInfo3 { get; set; }

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

        ButtonSoundChar = binaryReader.ReadUInt16();
        ButtonSoundInfo = null;
        if (ButtonSoundChar != 0)
        {
            ButtonSoundInfo = new SoundInfo();
            ButtonSoundInfo.ReadData(binaryReader);
        }

        ButtonSoundChar1 = binaryReader.ReadUInt16();
        ButtonSoundInfo1 = null;
        if (ButtonSoundChar1 != 0)
        {
            ButtonSoundInfo1 = new SoundInfo();
            ButtonSoundInfo1.ReadData(binaryReader);
        }

        ButtonSoundChar2 = binaryReader.ReadUInt16();
        ButtonSoundInfo2 = null;
        if (ButtonSoundChar2 != 0)
        {
            ButtonSoundInfo2 = new SoundInfo();
            ButtonSoundInfo2.ReadData(binaryReader);
        }

        ButtonSoundChar3 = binaryReader.ReadUInt16();
        ButtonSoundInfo3 = null;
        if (ButtonSoundChar3 != 0)
        {
            ButtonSoundInfo3 = new SoundInfo();
            ButtonSoundInfo3.ReadData(binaryReader);
        }
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>Size of this object</returns>
    protected int GetSizeOf()
    {
        var res = 10;
        if (ButtonSoundChar != 0 && ButtonSoundInfo != null)
            res += ButtonSoundInfo.GetSizeOf();
        if (ButtonSoundChar1 != 0 && ButtonSoundInfo1 != null)
            res += ButtonSoundInfo1.GetSizeOf();
        if (ButtonSoundChar2 != 0 && ButtonSoundInfo2 != null)
            res += ButtonSoundInfo2.GetSizeOf();
        if (ButtonSoundChar3 != 0 && ButtonSoundInfo3 != null)
            res += ButtonSoundInfo3.GetSizeOf();

        return res;
    }

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void UpdateData(byte version)
    {
        if (version < 2)
            return;

        var m = new MemoryStream();
        var w = new BufferedBinaryWriter(m);

        var rh = new RecordHeader(TagCode, GetSizeOf());

        rh.WriteTo(w);
        w.Write(CharacterId);

        w.Write(ButtonSoundChar);
        if (ButtonSoundChar != 0 && ButtonSoundInfo != null)
            ButtonSoundInfo.WriteTo(w);

        w.Write(ButtonSoundChar1);
        if (ButtonSoundChar1 != 0 && ButtonSoundInfo1 != null)
            ButtonSoundInfo1.WriteTo(w);

        w.Write(ButtonSoundChar2);
        if (ButtonSoundChar2 != 0 && ButtonSoundInfo2 != null)
            ButtonSoundInfo2.WriteTo(w);

        w.Write(ButtonSoundChar3);
        if (ButtonSoundChar3 != 0 && ButtonSoundInfo3 != null)
            ButtonSoundInfo3.WriteTo(w);

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
        writer.WriteStartElement("DefineButtonSoundTag");
        writer.WriteElementString("ButtonId", CharacterId.ToString());

        writer.WriteElementString("ButtonSoundChar", ButtonSoundChar.ToString());
        if (ButtonSoundChar != 0 && ButtonSoundInfo != null)
            ButtonSoundInfo.Serialize(writer);

        writer.WriteElementString("ButtonSoundChar1", ButtonSoundChar1.ToString());
        if (ButtonSoundChar1 != 0 && ButtonSoundInfo1 != null)
            ButtonSoundInfo1.Serialize(writer);

        writer.WriteElementString("ButtonSoundChar2", ButtonSoundChar2.ToString());
        if (ButtonSoundChar2 != 0 && ButtonSoundInfo2 != null)
            ButtonSoundInfo2.Serialize(writer);

        writer.WriteElementString("ButtonSoundChar3", ButtonSoundChar3.ToString());
        if (ButtonSoundChar3 != 0 && ButtonSoundInfo3 != null)
            ButtonSoundInfo3.Serialize(writer);
        writer.WriteEndElement();
    }

    #endregion
}