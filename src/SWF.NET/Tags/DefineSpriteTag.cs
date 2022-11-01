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
///     DefineSpriteTag defines a movie clip that animates
///     shapes within a movie.
/// </summary>
/// <remarks>
///     <p>
///         It contains an array of movie objects that define the
///         placement of shapes, buttons, text and images and the
///         order in which they are displayed through a time-line
///         that is separate from the parent movie.
///     </p>
///     <p>
///         Although a movie clip contains the commands that instructs
///         the Flash Player on how to animate the clip it cannot
///         contain any new definitions of objects. All definitions
///         must be in the main movie. All objects referred to by
///         the movie clip must be also defined in the main movie
///         before they can be used.
///     </p>
///     <p>
///         When using the DefineSpriteTag object can only contain
///         objects from the following classes:
///         <see cref="ShowFrameTag">ShowFrameTag</see>,
///         <see cref="PlaceObjectTag">PlaceObjectTag</see>,
///         <see cref="PlaceObject2Tag">PlaceObject2Tag</see>,
///         <see cref="RemoveObjectTag">RemoveObjectTag</see>,
///         <see cref="RemoveObject2Tag">RemoveObject2Tag</see>,
///         <see cref="DoActionTag">DoActionTag</see>,
///         <see cref="StartSoundTag">StartSoundTag</see>,
///         <see cref="FrameLabelTag">FrameLabelTag</see>,
///         <see cref="SoundStreamHeadTag">SoundStreamHeadTag</see>,
///         <see cref="SoundStreamHead2Tag">SoundStreamHead2Tag</see>,
///         <see cref="SoundStreamBlockTag">SoundStreamBlockTag</see>
///         or <see cref="VideoFrameTag">VideoFrameTag</see>.
///         Other objects are not allowed.
///     </p>
///     <p>
///         This tag was introduced in Flash 3.
///     </p>
/// </remarks>
public class DefineSpriteTag : BaseTag, DefineTag, IBaseTagContainer
{
    #region Members

    /// <summary>
    ///     Sprite ID
    /// </summary>
    private ushort spriteId;

    /// <summary>
    ///     Frame count
    /// </summary>
    private ushort frameCount;

    /// <summary>
    ///     inner tags
    /// </summary>
    private BaseTagCollection tagList;

    /// <summary>
    ///     action count including inner tags´ actions
    /// </summary>
    private readonly int _actionCount;

    /// <summary>
    ///     tag index for action block index
    /// </summary>
    private readonly int[] tagForAction;

    /// <summary>
    ///     contains action block counts for inner tags
    /// </summary>
    private readonly int[] tagOffset;

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="DefineSpriteTag" /> instance.
    /// </summary>
    public DefineSpriteTag() =>
        Init();

    /// <summary>
    ///     Creates a new <see cref="DefineSpriteTag" /> instance.
    /// </summary>
    /// <param name="spriteId">Sprite id. That's a unique identifier, in the range 1..65535, for the movie clip.</param>
    /// <param name="frameCount">Frame count. </param>
    /// <param name="tags">Tags. That's an array of BaseTag objects that define the commands that are executed by the Flash Player to animate the movie clip.</param>
    public DefineSpriteTag(ushort spriteId, ushort frameCount, BaseTagCollection tags)
    {
        Init();
        this.spriteId = spriteId;
        this.frameCount = frameCount;
        tagList = tags;

        if (tagList == null || tagList.Count == 0)
            return;

        _actionCount = 0;
        foreach (BaseTag b in tagList)
            if (b != null)
                _actionCount += b.ActionRecCount;

        tagForAction = new int[_actionCount];
        tagOffset = new int[tagList.Count];

        var actionIdx = 0;
        for (var i = 0; i < tagList.Count; i++)
        {
            if (tagList[i] == null)
                continue;
            tagOffset[i] = actionIdx;
            var count = tagList[i].ActionRecCount;
            if (count > 0)
            {
                for (var j = 0; j < count; j++) tagForAction[actionIdx + j] = i;
                actionIdx += count;
            }
        }
    }

    /// <summary>
    ///     Inits this instance.
    /// </summary>
    protected void Init()
    {
        _tagCode = (int)TagCodeEnum.DefineSprite;
        tagList = new BaseTagCollection();
    }

    #endregion

    #region Properties

    /// <summary>
    ///     see <see cref="DefineTag" />
    /// </summary>
    public ushort CharacterId
    {
        get => spriteId;
        set => spriteId = value;
    }

    /// <summary>
    ///     Gets or sets the frame count.
    ///     That's the total frame count of this movie clip.
    /// </summary>
    public ushort FrameCount
    {
        get => frameCount;
        set => frameCount = value;
    }

    /// <summary>
    ///     Gets the tags.
    ///     That's an array of BaseTag objects that define the
    ///     commands that are executed by the Flash Player to animate
    ///     the movie clip.
    /// </summary>
    public BaseTagCollection Tags =>
        tagList;

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override int ActionRecCount =>
        _actionCount;

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override byte[] this[int index]
    {
        get
        {
            if (index < 0 || index >= ActionRecCount) return null;

            var offset = index - tagOffset[tagForAction[index]];
            return tagList[tagForAction[index]][offset];
        }

        set
        {
            if (index > -1 && index < ActionRecCount)
            {
                var offset = index - tagOffset[tagForAction[index]];
                tagList[tagForAction[index]][offset] = value;
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Adds the specified child tag.
    /// </summary>
    /// <param name="childTag">Child tag.</param>
    public void Add(BaseTag childTag) =>
        Tags.Add(childTag);

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void ReadData(byte version, BufferedBinaryReader binaryReader)
    {
        var rh = new RecordHeader();
        rh.ReadData(binaryReader);

        // inner tags
        var endPos = binaryReader.BaseStream.Position + rh.TagLength;

        // stuff before inner tags, just read it and dont look any further
        spriteId = binaryReader.ReadUInt16();
        frameCount = binaryReader.ReadUInt16();

        while (binaryReader.BaseStream.Position < endPos)
        {
            var b = SwfReader.ReadTag(version, binaryReader, tagList);
            tagList.Add(b);
        }
    }

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void UpdateData(byte version)
    {
        if (version < 3)
            return;

        // update inner tags
        var len = 0;

        var tags = tagList.GetEnumerator();
        while (tags.MoveNext())
        {
            var tag = (BaseTag)tags.Current;
            tag.UpdateData(version);
            len += tag.Data.Length;
        }

        var m = new MemoryStream();
        var w = new BufferedBinaryWriter(m);

        var rh = new RecordHeader(TagCode, len + 4);

        rh.WriteTo(w);
        w.Write(spriteId);
        w.Write(frameCount);

        tags = tagList.GetEnumerator();
        while (tags.MoveNext())
        {
            var tag = (BaseTag)tags.Current;
            w.Write(tag.Data);
        }

        w.Flush();
        // update data
        _data = m.ToArray();
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("DefineSpriteTag");
        writer.WriteAttributeString("SpriteId", spriteId.ToString());
        writer.WriteAttributeString("FrameCount", frameCount.ToString());

        writer.WriteStartElement("Tags");
        var tags = tagList.GetEnumerator();
        while (tags.MoveNext())
        {
            var tag = (BaseTag)tags.Current;
            tag.Serialize(writer);
        }

        writer.WriteEndElement();

        writer.WriteEndElement();
    }

    #endregion
}