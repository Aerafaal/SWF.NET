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
using SWF.NET.Utils;

namespace SWF.NET.Tags;

/// <summary>
///     DoActionTag is used to specify a sequence of actions that are executed
///     when a frame is displayed.
/// </summary>
/// <remarks>
///     <p>
///         To define the actions for a given frame the DoActionTag object should be
///         added to a movie after the previous frame is displayed but before the
///         ShowFrameTag object that displays the 'current' frame and triggers the
///         actions to be executed.
///     </p>
///     <p>
///         Only one DoActionTag object can be used to specify the actions for a
///         given frame. If more than one DoActionTag object is added in a single
///         frame only the actions contained in the last DoActionTag object
///         (before the ShowFrameTag object) will be executed when the frame is
///         displayed. The other DoActionTag objects will be ignored.
///     </p>
///     <p>
///         This tag was introduced in Flash 1.
///     </p>
/// </remarks>
public class DoActionTag : BaseTag
{
    #region Members

    /// <summary>
    ///     bytecode block
    /// </summary>
    private byte[] actionRecord;

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="DoActionTag" /> instance.
    /// </summary>
    public DoActionTag() =>
        _tagCode = (int)TagCodeEnum.DoAction;

    /// <summary>
    ///     Creates a new <see cref="DoActionTag" /> instance.
    /// </summary>
    /// <param name="actionRecord">Swf bytecode action block.</param>
    public DoActionTag(byte[] actionRecord)
    {
        this.actionRecord = actionRecord;
        _tagCode = (int)TagCodeEnum.DoAction;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override int ActionRecCount =>
        1;

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override byte[] this[int index]
    {
        get => actionRecord;
        set => actionRecord = value;
    }

    #endregion

    #region Methods

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void ReadData(byte version, BufferedBinaryReader binaryReader)
    {
        var rh = new RecordHeader();
        rh.ReadData(binaryReader);

        var lenght = Convert.ToInt32(rh.TagLength);
        actionRecord = binaryReader.ReadBytes(lenght);
    }

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void UpdateData(byte version)
    {
        var m = new MemoryStream();
        var w = new BufferedBinaryWriter(m);

        var rh = new RecordHeader(TagCode, actionRecord.Length);

        rh.WriteTo(w);
        w.Write(actionRecord);

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
        writer.WriteStartElement("DoActionTag");
        writer.WriteEndElement();
    }

    #endregion
}