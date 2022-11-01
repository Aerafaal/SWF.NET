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
///     DefineEditTextTag defines an editable text field.
/// </summary>
/// <remarks>
///     <p>
///         The value entered into the text field is assigned to a specified
///         variable allowing the creation of forms to accept values entered
///         by a person viewing the Flash file.
///     </p>
///     <p>
///         The class contains a complex set of attributes which allows a high
///         degree of control over how a text field is displayed.
///     </p>
///     <p>
///         Additional layout information for the spacing of the text relative
///         to the text field borders can also be specified.
///     </p>
///     <p>
///         Setting the HTML flag to true allows text marked up with a limited
///         set of HTML tags to be displayed in the text field.
///     </p>
///     <p>
///         This tag was introduced in Flash 4.
///     </p>
/// </remarks>
public class DefineEditTextTag : BaseTag, DefineTag, DefineTargetTag
{
    #region Members

    #endregion

    #region Ctor & Init

    /// <summary>
    ///     constructor.
    /// </summary>
    public DefineEditTextTag() =>
        Init();

    /// <summary>
    ///     Inits this instance.
    /// </summary>
    protected void Init()
    {
        _tagCode = (int)TagCodeEnum.DefineEditText;
        VariableName = string.Empty;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the max lenght.
    /// </summary>
    /// <value></value>
    public ushort MaxLenght { get; set; }

    /// <summary>
    ///     Gets or sets the left margin.
    /// </summary>
    /// <value></value>
    public ushort LeftMargin { get; set; }

    /// <summary>
    ///     Gets or sets the right margin.
    /// </summary>
    /// <value></value>
    public ushort RightMargin { get; set; }

    /// <summary>
    ///     Gets or sets the indent.
    /// </summary>
    /// <value></value>
    public ushort Indent { get; set; }

    /// <summary>
    ///     Gets or sets the leading.
    /// </summary>
    /// <value></value>
    public ushort Leading { get; set; }

    /// <summary>
    ///     Gets or sets the name of the variable.
    /// </summary>
    /// <value></value>
    public string VariableName { get; set; }

    /// <summary>
    ///     Gets or sets the align.
    /// </summary>
    /// <value></value>
    public byte Align { get; set; }

    /// <summary>
    ///     Gets or sets the height of the font.
    /// </summary>
    /// <value></value>
    public ushort FontHeight { get; set; }

    /// <summary>
    ///     Gets or sets the font id.
    /// </summary>
    /// <value></value>
    public ushort FontId { get; set; }

    /// <summary>
    ///     Target tag's character id
    /// </summary>
    /// <value></value>
    public ushort TargetCharacterId
    {
        get => FontId;
        set => FontId = value;
    }

    /// <summary>
    ///     Gets or sets the rect.
    /// </summary>
    /// <value></value>
    public Rect Rect { get; set; }

    /// <summary>
    ///     Gets or sets the color of the text.
    /// </summary>
    /// <value></value>
    public RGBA TextColor { get; set; }

    /// <summary>
    ///     Gets or sets the initial text.
    /// </summary>
    /// <value></value>
    public string InitialText { get; set; }

    /// <summary>
    ///     see <see cref="DefineTag" />
    /// </summary>
    public ushort CharacterId { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether [word wrap].
    /// </summary>
    public bool WordWrap { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="DefineEditTextTag" /> is multiline.
    /// </summary>
    public bool Multiline { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="DefineEditTextTag" /> is password.
    /// </summary>
    public bool Password { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether [read only].
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether [auto size].
    /// </summary>
    public bool AutoSize { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether [no select].
    /// </summary>
    public bool NoSelect { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="DefineEditTextTag" /> is border.
    /// </summary>
    public bool Border { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="DefineEditTextTag" /> is HTML.
    /// </summary>
    public bool Html { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether [used outlines].
    /// </summary>
    public bool UsedOutlines { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Gets a value indicating whether this instance has max length.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance has max length; otherwise, <c>false</c>.
    /// </value>
    private bool HasMaxLength =>
        MaxLenght != 0;

    /// <summary>
    ///     Gets a value indicating whether this instance has text.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance has text; otherwise, <c>false</c>.
    /// </value>
    private bool HasText =>
        InitialText != null;

    /// <summary>
    ///     Gets a value indicating whether this instance has font.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance has font; otherwise, <c>false</c>.
    /// </value>
    private bool HasFont =>
        FontId != 0 && FontHeight != 0;

    /// <summary>
    ///     Gets a value indicating whether this instance has text color.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance has text color; otherwise, <c>false</c>.
    /// </value>
    private bool HasTextColor =>
        TextColor != null;

    /// <summary>
    ///     Gets a value indicating whether this instance has layout.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance has layout; otherwise, <c>false</c>.
    /// </value>
    private bool HasLayout =>
        Align != 0 ||
        LeftMargin != 0 ||
        RightMargin != 0 ||
        Indent != 0 ||
        Leading != 0;

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void ReadData(byte version, BufferedBinaryReader binaryReader)
    {
        var rh = new RecordHeader();
        rh.ReadData(binaryReader);

        CharacterId = binaryReader.ReadUInt16();
        Rect = new Rect();
        Rect.ReadData(binaryReader);

        var ba = BitParser.GetBitValues(new byte[1] { binaryReader.ReadByte() });

        var hasText = ba.Get(0); //binaryReader.ReadBoolean();
        WordWrap = ba.Get(1); //binaryReader.ReadBoolean();
        Multiline = ba.Get(2); //binaryReader.ReadBoolean();
        Password = ba.Get(3); //binaryReader.ReadBoolean();
        ReadOnly = ba.Get(4); //binaryReader.ReadBoolean();
        var hasTextColor = ba.Get(5); //binaryReader.ReadBoolean();
        var hasMaxLength = ba.Get(6); //binaryReader.ReadBoolean();
        var hasFont = ba.Get(7); //binaryReader.ReadBoolean();
        //binaryReader.SynchBits();

        ba = BitParser.GetBitValues(new byte[1] { binaryReader.ReadByte() });
        //binaryReader.ReadBoolean(); //Reserved
        AutoSize = ba.Get(1); //binaryReader.ReadBoolean();
        var hasLayout = ba.Get(2); //binaryReader.ReadBoolean();
        NoSelect = ba.Get(3); //binaryReader.ReadBoolean();
        Border = ba.Get(4); //binaryReader.ReadBoolean();
        //binaryReader.ReadBoolean(); //Reserved
        Html = ba.Get(6); //binaryReader.ReadBoolean();
        UsedOutlines = ba.Get(7); //binaryReader.ReadBoolean();

        if (hasFont)
        {
            FontId = binaryReader.ReadUInt16();
            FontHeight = binaryReader.ReadUInt16();
        }

        if (hasTextColor)
        {
            TextColor = new RGBA();
            TextColor.ReadData(binaryReader);
        }

        if (hasMaxLength)
            MaxLenght = binaryReader.ReadUInt16();

        if (hasLayout)
        {
            Align = binaryReader.ReadByte();
            LeftMargin = binaryReader.ReadUInt16();
            RightMargin = binaryReader.ReadUInt16();
            Indent = binaryReader.ReadUInt16();
            Leading = binaryReader.ReadUInt16();
        }

        VariableName = binaryReader.ReadString();
        if (hasText)
            InitialText = binaryReader.ReadString();
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf()
    {
        var res = 2;
        if (Rect != null)
            res += Rect.GetSizeOf();
        res += 2;
        if (HasFont)
            res += 4;
        if (HasTextColor)
            res += TextColor.GetSizeOf();
        if (HasMaxLength)
            res += 2;
        if (HasLayout)
            res += 9;
        res += VariableName.Length + 1;
        if (HasText)
            res += InitialText.Length + 1;
        return res;
    }

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void UpdateData(byte version)
    {
        if (version < 4)
            return;

        var m = new MemoryStream();
        var w = new BufferedBinaryWriter(m);
        var rh = new RecordHeader(TagCode, GetSizeOf());
        rh.WriteTo(w);

        w.Write(CharacterId);
        Rect.WriteTo(w);

        w.SynchBits();
        if (InitialText != null && InitialText.Length > 0)
            w.WriteBoolean(true);
        else
            w.WriteBoolean(false);
        w.WriteBoolean(WordWrap);
        w.WriteBoolean(Multiline);
        w.WriteBoolean(Password);
        w.WriteBoolean(ReadOnly);
        if (TextColor != null)
            w.WriteBoolean(true);
        else
            w.WriteBoolean(false);
        w.WriteBoolean(HasMaxLength);
        w.WriteBoolean(HasFont);
        w.SynchBits();

        w.WriteBoolean(false);
        w.WriteBoolean(AutoSize);
        w.WriteBoolean(HasLayout);
        w.WriteBoolean(NoSelect);
        w.WriteBoolean(Border);
        w.WriteBoolean(false);
        w.WriteBoolean(Html);
        w.WriteBoolean(UsedOutlines);

        if (HasFont)
        {
            w.Write(FontId);
            w.Write(FontHeight);
        }

        if (HasTextColor)
            TextColor.WriteTo(w);
        if (HasMaxLength)
            w.Write(MaxLenght);
        if (HasLayout)
        {
            w.Write(Align);
            w.Write(LeftMargin);
            w.Write(RightMargin);
            w.Write(Indent);
            w.Write(Leading);
        }

        w.WriteString(VariableName);
        if (HasText)
            w.WriteString(InitialText);

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
        writer.WriteStartElement("DefineEditTextTag");
        writer.WriteElementString("CharacterId", CharacterId.ToString());
        Rect.Serialize(writer);
        writer.WriteElementString("WordWrap", WordWrap.ToString());
        writer.WriteElementString("Multiline", Multiline.ToString());
        writer.WriteElementString("Password", Password.ToString());
        writer.WriteElementString("ReadOnly", ReadOnly.ToString());
        writer.WriteElementString("HasMaxLength", HasMaxLength.ToString());
        writer.WriteElementString("HasFont", HasFont.ToString());
        writer.WriteElementString("AutoSize", AutoSize.ToString());
        writer.WriteElementString("HasLayout", HasLayout.ToString());
        writer.WriteElementString("NoSelect", NoSelect.ToString());
        writer.WriteElementString("Border", Border.ToString());
        writer.WriteElementString("Html", Html.ToString());
        writer.WriteElementString("UsedOutlines", UsedOutlines.ToString());

        if (HasFont)
        {
            writer.WriteElementString("FontId", FontId.ToString());
            writer.WriteElementString("FontHeight", FontHeight.ToString());
        }

        if (HasTextColor)
            TextColor.Serialize(writer);
        if (HasMaxLength)
            writer.WriteElementString("MaxLenght", MaxLenght.ToString());
        if (HasLayout)
        {
            writer.WriteElementString("Align", Align.ToString());
            writer.WriteElementString("LeftMargin", LeftMargin.ToString());
            writer.WriteElementString("RightMargin", RightMargin.ToString());
            writer.WriteElementString("Indent", Indent.ToString());
            writer.WriteElementString("Leading", Leading.ToString());
        }

        writer.WriteElementString("VariableName", VariableName);
        if (HasText)
            writer.WriteElementString("InitialText", InitialText);

        writer.WriteEndElement();
    }

    #endregion
}