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
using SWF.NET.Utils;

namespace SWF.NET.Tags;

/// <summary>
///     SetTabIndexTag class is used to set the tabbing order of text fields,
///     movie clips and buttons visible on the display list.
/// </summary>
/// <remarks>
///     <p>
///         This tag was introduced in Flash 7.
///     </p>
/// </remarks>
public class SetTabIndexTag : BaseTag
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="SetTabIndexTag" /> instance.
    /// </summary>
    public SetTabIndexTag() =>
        _tagCode = (int)TagCodeEnum.SetTabIndex;

    /// <summary>
    ///     constructor
    /// </summary>
    /// <param name="depth">depth of character</param>
    /// <param name="tabIndex">tab order value</param>
    public SetTabIndexTag(ushort depth, ushort tabIndex)
    {
        this.Depth = depth;
        this.TabIndex = tabIndex;

        _tagCode = (int)TagCodeEnum.SetTabIndex;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the depth.
    /// </summary>
    public ushort Depth { get; set; }

    /// <summary>
    ///     Gets or sets the tab index.
    /// </summary>
    public ushort TabIndex { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void ReadData(byte version, BufferedBinaryReader binaryReader)
    {
        var rh = new RecordHeader();
        rh.ReadData(binaryReader);

        Depth = binaryReader.ReadUInt16();
        TabIndex = binaryReader.ReadUInt16();
    }

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void UpdateData(byte version)
    {
        if (version < 7)
            return;

        var m = new MemoryStream();
        var w = new BufferedBinaryWriter(m);

        var rh = new RecordHeader(TagCode, 4);

        rh.WriteTo(w);
        w.Write(Depth);
        w.Write(TabIndex);

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
        writer.WriteStartElement("SetTabIndexTag");
        writer.WriteElementString("Depth", Depth.ToString());
        writer.WriteElementString("TabIndex", TabIndex.ToString());
        writer.WriteEndElement();
    }

    #endregion
}