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

using System.Collections;
using System.Xml;
using SWF.NET.Utils;

namespace SWF.NET.Tags.Types;

/// <summary>
///     ButtonRecord class
/// </summary>
public class ButtonRecord : ISwfSerializer, DefineTargetTag
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="ButtonRecord" /> instance.
    /// </summary>
    public ButtonRecord()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="ButtonRecord" /> instance.
    /// </summary>
    /// <param name="buttonStateHitTest">Button state hit test.</param>
    /// <param name="buttonStateDown">Button state down.</param>
    /// <param name="buttonStateOver">Button state over.</param>
    /// <param name="buttonStateUp">Button state up.</param>
    /// <param name="characterId">Character id.</param>
    /// <param name="placeDepth">Place depth.</param>
    /// <param name="placeMatrix">Place matrix.</param>
    /// <param name="colorTransform">Color transform.</param>
    public ButtonRecord(
        bool buttonStateHitTest,
        bool buttonStateDown,
        bool buttonStateOver,
        bool buttonStateUp,
        ushort characterId,
        ushort placeDepth,
        Matrix placeMatrix,
        CXFormWithAlphaData colorTransform)
    {
        ButtonStateHitTest = buttonStateHitTest;
        ButtonStateDown = buttonStateDown;
        ButtonStateOver = buttonStateOver;
        ButtonStateUp = buttonStateUp;
        TargetCharacterId = characterId;
        PlaceDepth = placeDepth;
        PlaceMatrix = placeMatrix;
        ColorTransform = colorTransform;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets a value indicating whether [button state hit test].
    /// </summary>
    public bool ButtonStateHitTest { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether [button state down].
    /// </summary>
    public bool ButtonStateDown { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether [button state over].
    /// </summary>
    public bool ButtonStateOver { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether [button state up].
    /// </summary>
    public bool ButtonStateUp { get; set; }

    /// <summary>
    ///     Gets or sets the character id.
    /// </summary>
    public ushort TargetCharacterId { get; set; }

    /// <summary>
    ///     Gets or sets the place depth.
    /// </summary>
    public ushort PlaceDepth { get; set; }

    /// <summary>
    ///     Gets or sets the place matrix.
    /// </summary>
    public Matrix PlaceMatrix { get; set; }

    /// <summary>
    ///     Gets or sets the color transform.
    /// </summary>
    public CXFormWithAlphaData ColorTransform { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    /// <param name="firstByte">First byte.</param>
    /// <param name="buttonType">Button type.</param>
    public void ReadData(
        BufferedBinaryReader binaryReader,
        byte firstByte,
        TagCodeEnum buttonType)
    {
        var ba = BitParser.GetBitValues(new byte[1] { firstByte });
        ButtonStateHitTest = ba.Get(4);
        ButtonStateDown = ba.Get(5);
        ButtonStateOver = ba.Get(6);
        ButtonStateUp = ba.Get(7);

        TargetCharacterId = binaryReader.ReadUInt16();
        PlaceDepth = binaryReader.ReadUInt16();
        PlaceMatrix = new Matrix();
        PlaceMatrix.ReadData(binaryReader);
        ColorTransform = null;

        if (buttonType == TagCodeEnum.DefineButton2)
        {
            ColorTransform = new CXFormWithAlphaData();
            ColorTransform.ReadData(binaryReader);
        }
    }

    /// <summary>
    ///     Gets the size.
    /// </summary>
    /// <returns>Size</returns>
    public int GetSizeOf()
    {
        var res = 5;
        if (PlaceMatrix != null)
            res += PlaceMatrix.GetSizeOf();
        if (ColorTransform != null)
            res += ColorTransform.GetSizeOf();
        return res;
    }

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    /// <param name="buttonType">Button type.</param>
    public void WriteTo(BufferedBinaryWriter writer, TagCodeEnum buttonType)
    {
        writer.WriteUBits(0, 4);
        writer.WriteBoolean(ButtonStateHitTest);
        writer.WriteBoolean(ButtonStateDown);
        writer.WriteBoolean(ButtonStateOver);
        writer.WriteBoolean(ButtonStateUp);

        writer.Write(TargetCharacterId);
        writer.Write(PlaceDepth);
        if (PlaceMatrix != null)
            PlaceMatrix.WriteTo(writer);
        if (ColorTransform != null && buttonType == TagCodeEnum.DefineButton2)
            ColorTransform.WriteTo(writer);
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("ButtonRecord");
        writer.WriteElementString("ButtonStateHitTest", ButtonStateHitTest.ToString());
        writer.WriteElementString("ButtonStateDown", ButtonStateDown.ToString());
        writer.WriteElementString("ButtonStateOver", ButtonStateOver.ToString());
        writer.WriteElementString("ButtonStateUp", ButtonStateUp.ToString());
        writer.WriteElementString("CharacterId", TargetCharacterId.ToString());
        writer.WriteElementString("PlaceDepth", PlaceDepth.ToString());
        if (PlaceMatrix != null)
            PlaceMatrix.Serialize(writer);
        if (ColorTransform != null)
            ColorTransform.Serialize(writer);
        writer.WriteEndElement();
    }

    #endregion
}

/// <summary>
///     ButtonRecordCollection class
/// </summary>
public class ButtonRecordCollection : CollectionBase, ISwfSerializer
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="ButtonRecordCollection" /> instance.
    /// </summary>
    public ButtonRecordCollection()
    {
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("ButtonRecordCollection");

        var buttonRecords = GetEnumerator();
        while (buttonRecords.MoveNext())
            ((ButtonRecord)buttonRecords.Current).Serialize(writer);

        writer.WriteEndElement();
    }

    #endregion

    #region Collection methods

    /// <summary>
    ///     Adds the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public ButtonRecord Add(ButtonRecord value)
    {
        List.Add(value);
        return value;
    }

    /// <summary>
    ///     Adds the range.
    /// </summary>
    /// <param name="values">Values.</param>
    public void AddRange(ButtonRecord[] values)
    {
        var val = values.GetEnumerator();
        while (val.MoveNext())
            Add((ButtonRecord)val.Current);
    }

    /// <summary>
    ///     Removes the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    public void Remove(ButtonRecord value)
    {
        if (List.Contains(value))
            List.Remove(value);
    }

    /// <summary>
    ///     Inserts the specified index.
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="value">Value.</param>
    public void Insert(int index, ButtonRecord value) =>
        List.Insert(index, value);

    /// <summary>
    ///     Containses the specified value.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public bool Contains(ButtonRecord value) =>
        List.Contains(value);

    /// <summary>
    ///     Gets or sets the <see cref="LineStyle" /> at the specified index.
    /// </summary>
    /// <value></value>
    public ButtonRecord this[int index]
    {
        get => (ButtonRecord)List[index];
        set => List[index] = value;
    }

    /// <summary>
    ///     Get the index of.
    /// </summary>
    /// <param name="value">Value.</param>
    /// <returns></returns>
    public int IndexOf(ButtonRecord value) =>
        List.IndexOf(value);

    #endregion
}