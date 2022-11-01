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

using System.Collections;
using System.IO;
using System.Xml;
using SWF.NET.Tags.Types;
using SWF.NET.Utils;

namespace SWF.NET.Tags;

/// <summary>
///     ExportAssetsTag  is used to export shapes and other objects
///     so they can be used in another Flash file.
/// </summary>
/// <remarks>
///     <p>
///         Since the identifier for an object is only unique within a given
///         Flash file, each object exported must be given a name so it can
///         referenced when it is imported.
///     </p>
///     <p>
///         This tag was introduced in Flash 5.
///     </p>
/// </remarks>
public class ExportAssetsTag : BaseTag, DefineTargetTagContainer
{
    #region Members

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the exported characters.
    /// </summary>
    public AssertCollection ExportedCharacters { get; }

    #endregion

    /// <summary>
    ///     Targets to.
    /// </summary>
    /// <param name="characterId">The character id.</param>
    /// <param name="history">The history.</param>
    /// <returns></returns>
    public bool TargetTo(ushort characterId, Hashtable history)
    {
        var values = ExportedCharacters.GetEnumerator();
        while (values.MoveNext())
        {
            var value = (Assert)values.Current;
            if (value.TargetCharacterId == characterId && history.Contains(value) == false)
                return true;
        }

        return false;
    }

    /// <summary>
    ///     Changeds the target.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <param name="newId">The new id.</param>
    /// <param name="history">The history.</param>
    public void ChangedTarget(ushort id, ushort newId, Hashtable history)
    {
        var values = ExportedCharacters.GetEnumerator();
        while (values.MoveNext())
        {
            var value = (Assert)values.Current;
            if (value.TargetCharacterId == id && history.Contains(value) == false)
            {
                value.TargetCharacterId = newId;
                history.Add(value, newId);
            }
        }
    }

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="ExportAssetsTag" /> instance.
    /// </summary>
    public ExportAssetsTag()
    {
        _tagCode = (int)TagCodeEnum.ExportAssets;
        ExportedCharacters = new AssertCollection();
    }

    /// <summary>
    ///     Creates a new <see cref="ExportAssetsTag" /> instance.
    /// </summary>
    /// <param name="exportedCharacters">Exported characters.</param>
    public ExportAssetsTag(Assert[] exportedCharacters)
    {
        ExportedCharacters = new AssertCollection();
        ExportedCharacters.AddRange(exportedCharacters);
        _tagCode = (int)TagCodeEnum.ExportAssets;
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="version">Version.</param>
    /// <param name="binaryReader">Binary reader.</param>
    public override void ReadData(byte version, BufferedBinaryReader binaryReader)
    {
        var rh = new RecordHeader();
        rh.ReadData(binaryReader);

        var count = binaryReader.ReadUInt16();
        ExportedCharacters.Clear();
        if (count > 0)
            for (var i = 0; i < count; i++)
            {
                var exportedCharacter = new Assert();
                exportedCharacter.ReadData(binaryReader);
                ExportedCharacters.Add(exportedCharacter);
            }
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf()
    {
        var lenght = 2;
        if (ExportedCharacters != null)
            lenght += Assert.GetSizeOf(ExportedCharacters);
        return lenght;
    }

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void UpdateData(byte version)
    {
        if (version < 5)
            return;

        var m = new MemoryStream();
        var w = new BufferedBinaryWriter(m);

        var rh = new RecordHeader(TagCode, GetSizeOf());
        rh.WriteTo(w);

        if (ExportedCharacters != null)
            w.Write((ushort)ExportedCharacters.Count);
        else
            w.Write((ushort)0);

        if (ExportedCharacters != null)
        {
            var assertEnu = ExportedCharacters.GetEnumerator();
            while (assertEnu.MoveNext())
            {
                var assert = (Assert)assertEnu.Current;
                assert.WriteTo(w);
            }
        }

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
        writer.WriteStartElement("ExportAssetsTag");
        foreach (Assert ass in ExportedCharacters)
            ass.Serialize(writer);
        writer.WriteEndElement();
    }

    #endregion
}