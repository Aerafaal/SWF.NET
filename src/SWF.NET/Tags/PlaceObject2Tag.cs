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
///     PlaceObject2Tag is used to add and manipulate objects (shape, button, etc.)
///     on the Flash Player's display list.
/// </summary>
/// <remarks>
///     <p>
///         PlaceObject2Tag supersedes the PlaceObjectTag class providing more functionality
///         and easier manipulation of  objects in the display list through the
///         following operations:
///         <ul>
///             <li>Place a new shape on the display list.</li>
///             <li>
///                 Change an existing shape by moving it to new location or changing its
///                 appearance.
///             </li>
///             <li>Replace an existing shape with a another.</li>
///             <li>Define clipping layers to mask objects displayed in front of a shape.</li>
///             <li>Control the morphing process that changes one shape into another.</li>
///             <li>Assign names to objects rather than using their identifiers.</li>
///             <li>
///                 Define the sequence of actions that are executed when an event occurs
///                 in movie clip.
///             </li>
///         </ul>
///     </p>
///     <p>
///         Since only one object can be placed on a given layer an existing object
///         on the display list can be identified by the layer it is displayed on rather
///         than its identifier. Therefore Layer is the only required attribute.
///         The remaining attributes are optional according to the different operation
///         being performed:
///         <ul>
///             <li>
///                 If an existing object on the display list is being modified then only the
///                 layer number is required. Previously in the PlaceObjectTag class both the
///                 identifier and the layer number were required.
///             </li>
///             <li>
///                 If no coordinate transform is applied to the shape (the default is a
///                 unity transform that does not change the shape) then it is not encoded.
///             </li>
///             <li>
///                 Similarly if no colour transform is applied to the shape (the default
///                 is a unity transform that does not change the shape's colour) then it is
///                 not encoded.
///             </li>
///             <li>
///                 If a shape is not being morphed then the ratio attribute may be left at
///                 its default value (-1.0).
///             </li>
///             <li>
///                 If a shape is not used to define a clipping area then the depth attribute
///                 may be left at its default value (0).
///             </li>
///             <li>
///                 If a name is net assigned to an object the name attribute may be left its
///                 default value (an empty string).
///             </li>
///             <li>
///                 If no events are being defined for a movie clip then the array of ClipEvent
///                 object may be left empty.
///             </li>
///         </ul>
///     </p>
///     <p>
///         The class provides a range of constructors which define different subsets of the
///         attributes according to the type of operation that will be performed on an object
///         in the Flash Player's display list. If an attribute is not specified in a
///         constructor then it will be assigned a default value and will be omitted when the
///         object is encoded.
///     </p>
///     <p>
///         This tag was introduced in Flash 3.
///     </p>
/// </remarks>
public class PlaceObject2Tag : BaseTag, DefineTargetTag
{
    #region Members

    private string name;
    private ushort clipDepth;
    private ClipActionRec[] clipActions;
    private byte[] actionHead;

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="PlaceObject2Tag" /> instance.
    /// </summary>
    public PlaceObject2Tag() =>
        _tagCode = (int)TagCodeEnum.PlaceObject2;

    /// <summary>
    ///     Creates a new <see cref="PlaceObject2Tag" /> instance.
    /// </summary>
    /// <param name="characterId">Character id.</param>
    /// <param name="depth">Depth.</param>
    public PlaceObject2Tag(ushort characterId, int depth)
    {
        TargetCharacterId = characterId;
        Depth = (ushort)depth;
        _tagCode = (int)TagCodeEnum.PlaceObject2;
    }

    /// <summary>
    ///     Creates a new <see cref="PlaceObject2Tag" /> instance.
    /// </summary>
    /// <param name="characterId">Character id.</param>
    /// <param name="depth">Depth.</param>
    /// <param name="xLocation">X location.</param>
    /// <param name="yLocation">Y location.</param>
    public PlaceObject2Tag(ushort characterId, int depth, int xLocation, int yLocation)
    {
        TargetCharacterId = characterId;
        Depth = (ushort)depth;
        Matrix = new Matrix(xLocation, yLocation);
        _tagCode = (int)TagCodeEnum.PlaceObject2;
    }

    /// <summary>
    ///     Creates a new <see cref="PlaceObject2Tag" /> instance.
    /// </summary>
    /// <param name="characterId">Character id.</param>
    /// <param name="depth">Depth.</param>
    /// <param name="xLocation">X location.</param>
    /// <param name="yLocation">Y location.</param>
    /// <param name="placeFlagMove">Place flag move.</param>
    public PlaceObject2Tag(ushort characterId, int depth, int xLocation, int yLocation, bool placeFlagMove)
    {
        TargetCharacterId = characterId;
        Depth = (ushort)depth;
        Matrix = new Matrix(xLocation, yLocation);
        Move = placeFlagMove;
        _tagCode = (int)TagCodeEnum.PlaceObject2;
    }

    /// <summary>
    ///     Creates a new <see cref="PlaceObject2Tag" /> instance.
    /// </summary>
    /// <param name="characterId">Character id.</param>
    /// <param name="depth">Depth.</param>
    /// <param name="transformMatrix">Transform matrix.</param>
    public PlaceObject2Tag(ushort characterId, int depth, Matrix transformMatrix)
    {
        TargetCharacterId = characterId;
        Depth = (ushort)depth;
        Matrix = transformMatrix;
        _tagCode = (int)TagCodeEnum.PlaceObject2;
    }

    /// <summary>
    ///     Creates a new <see cref="PlaceObject2Tag" /> instance.
    /// </summary>
    /// <param name="characterId">Character id.</param>
    /// <param name="depth">Depth.</param>
    /// <param name="colorTransform">Color transform.</param>
    public PlaceObject2Tag(ushort characterId, int depth, CXFormWithAlphaData colorTransform)
    {
        TargetCharacterId = characterId;
        Depth = (ushort)depth;
        ColorTransform = colorTransform;
        Move = true;
        _tagCode = (int)TagCodeEnum.PlaceObject2;
    }

    /// <summary>
    ///     Creates a new <see cref="PlaceObject2Tag" /> instance.
    /// </summary>
    /// <param name="depth">Depth.</param>
    /// <param name="colorTransform">Color transform.</param>
    public PlaceObject2Tag(int depth, CXFormWithAlphaData colorTransform)
    {
        Depth = (ushort)depth;
        ColorTransform = colorTransform;
        Move = true;
        _tagCode = (int)TagCodeEnum.PlaceObject2;
    }

    /// <summary>
    ///     Creates a new <see cref="PlaceObject2Tag" /> instance.
    /// </summary>
    /// <param name="characterId">Character id.</param>
    /// <param name="depth">Depth.</param>
    /// <param name="ratio">Ratio.</param>
    /// <param name="xLocation">X location.</param>
    /// <param name="yLocation">Y location.</param>
    public PlaceObject2Tag(ushort characterId, int depth, float ratio, int xLocation, int yLocation)
    {
        TargetCharacterId = characterId;
        Depth = (ushort)depth;
        Ratio = ratio;
        Matrix = new Matrix(xLocation, yLocation);
        _tagCode = (int)TagCodeEnum.PlaceObject2;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="PlaceObject2Tag" /> is move.
    /// </summary>
    /// <value>
    ///     <c>true</c> if move; otherwise, <c>false</c>.
    /// </value>
    public bool Move { get; set; }

    /// <summary>
    ///     Gets or sets the depth.
    /// </summary>
    /// <value></value>
    public ushort Depth { get; set; }

    /// <summary>
    ///     Gets or sets the color transform.
    /// </summary>
    /// <value></value>
    public CXFormWithAlphaData ColorTransform { get; set; }

    /// <summary>
    ///     Gets or sets the character id.
    /// </summary>
    /// <value></value>
    public ushort TargetCharacterId { get; set; }

    /// <summary>
    ///     Gets or sets the ratio.
    /// </summary>
    /// <value></value>
    public float Ratio { get; set; }

    /// <summary>
    ///     Gets or sets the matrix.
    /// </summary>
    /// <value></value>
    public Matrix Matrix { get; set; }

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override int ActionRecCount
    {
        get
        {
            if (clipActions != null)
                return clipActions.Length;
            return 0;
        }
    }

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override byte[] this[int index]
    {
        get => clipActions[index].ActionRecord;
        set => clipActions[index].ActionRecord = value;
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

        var placeFlagHasClipActions = binaryReader.ReadBoolean();
        var placeFlagHasClipDepth = binaryReader.ReadBoolean();
        var placeFlagHasName = binaryReader.ReadBoolean();
        var placeFlagHasRatio = binaryReader.ReadBoolean();
        var placeFlagHasColorTransform = binaryReader.ReadBoolean();
        var placeFlagHasMatrix = binaryReader.ReadBoolean();
        var placeFlagHasCharacter = binaryReader.ReadBoolean();
        Move = binaryReader.ReadBoolean();

        Depth = binaryReader.ReadUInt16();
        TargetCharacterId = 0;
        if (placeFlagHasCharacter)
            TargetCharacterId = binaryReader.ReadUInt16();
        Matrix = null;
        if (placeFlagHasMatrix)
        {
            Matrix = new Matrix();
            Matrix.ReadData(binaryReader);
        }

        ColorTransform = null;
        if (placeFlagHasColorTransform)
        {
            ColorTransform = new CXFormWithAlphaData();
            ColorTransform.ReadData(binaryReader);
        }

        Ratio = 0;
        if (placeFlagHasRatio)
            Ratio = binaryReader.ReadUInt16() / 65535.0f;
        name = null;
        if (placeFlagHasName)
            name = binaryReader.ReadString();
        clipDepth = 0;
        if (placeFlagHasClipDepth)
            clipDepth = binaryReader.ReadUInt16();

        // get bytecode actions
        clipActions = null;
        if (placeFlagHasClipActions)
        {
            // different behaviour for Flash 6+
            actionHead = version >= 6 ? binaryReader.ReadBytes(6) : binaryReader.ReadBytes(4);
            // read clip action records to list
            var clpAc = new ArrayList();
            //ClipActionRec a = null;
            var res = true;
            do
            {
                var action = new ClipActionRec();
                res = action.ReadData(binaryReader, version);
                if (res)
                    clpAc.Add(action);
            }
            while (res);

            // copy list to array
            clipActions = new ClipActionRec[clpAc.Count];
            clpAc.CopyTo(clipActions, 0);
        }
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns></returns>
    public int GetSizeOf(byte version)
    {
        var res = 3;
        if (HasCharacter())
            res += 2;
        if (HasMatrix())
            res += Matrix.GetSizeOf();
        if (HasColorTransform())
            res += ColorTransform.GetSizeOf();
        if (HasRatio())
            res += 2;
        if (HasName())
            res += name.Length + 1;
        if (HasClipDepth())
            res += 2;
        if (HasClipActions())
        {
            res += actionHead.Length;
            foreach (var clip in clipActions)
                res += clip.GetData(version).Length;
            if (version <= 5)
                res += 2;
            else
                res += 4;
        }

        return res;
    }

    /// <summary>
    ///     see <see cref="BaseTag">base class</see>
    /// </summary>
    public override void UpdateData(byte version)
    {
        if (version < 3)
            return;

        var m = new MemoryStream();
        var w = new BufferedBinaryWriter(m);

        var rh = new RecordHeader(TagCode, GetSizeOf(version));
        rh.WriteTo(w);

        w.WriteBoolean(HasClipActions());
        w.WriteBoolean(HasClipDepth());
        w.WriteBoolean(HasName());
        w.WriteBoolean(HasRatio());
        w.WriteBoolean(HasColorTransform());
        w.WriteBoolean(HasMatrix());
        w.WriteBoolean(HasCharacter());
        w.WriteBoolean(Move);

        w.Write(Depth);
        if (HasCharacter())
            w.Write(TargetCharacterId);
        if (HasMatrix())
            Matrix.WriteTo(w);
        if (HasColorTransform())
            ColorTransform.WriteTo(w);
        if (HasRatio())
            w.Write((ushort)(Ratio * 65535.0f));
        if (HasName())
            w.WriteString(name);
        if (HasClipDepth())
            w.Write(clipDepth);

        if (HasClipActions())
        {
            w.Write(actionHead);
            // ClipActionRecords
            foreach (var clpA in clipActions)
                w.Write(clpA.GetData(version));
            // ClipActionRecords end
            if (version >= 6)
                w.Write(0);
            else
                w.Write((short)0);
        }

        w.Flush();
        _data = m.ToArray();
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("PlaceObject2Tag");
        writer.WriteElementString("PlaceFlagHasClipActions", HasClipActions().ToString());
        writer.WriteElementString("PlaceFlagHasClipDepth", HasClipDepth().ToString());
        writer.WriteElementString("PlaceFlagHasName", HasName().ToString());
        writer.WriteElementString("PlaceFlagHasRatio", HasRatio().ToString());
        writer.WriteElementString("PlaceFlagHasColorTransform", HasColorTransform().ToString());
        writer.WriteElementString("PlaceFlagHasMatrix", HasMatrix().ToString());
        writer.WriteElementString("PlaceFlagHasCharacter", HasCharacter().ToString());
        writer.WriteElementString("PlaceFlagMove", Move.ToString());
        writer.WriteElementString("Depth", Depth.ToString());

        if (HasCharacter())
            writer.WriteElementString("CharacterId", TargetCharacterId.ToString());
        if (HasMatrix())
            Matrix.Serialize(writer);
        if (HasColorTransform())
            ColorTransform.Serialize(writer);
        if (HasRatio())
            writer.WriteElementString("Ratio", Ratio.ToString());
        if (HasName())
            writer.WriteElementString("Name", name);
        if (HasClipDepth())
            writer.WriteElementString("ClipDepth", clipDepth.ToString());

        writer.WriteEndElement();
    }

    /// <summary>
    ///     Determines whether this instance has matrix.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this instance has matrix; otherwise, <c>false</c>.
    /// </returns>
    private bool HasMatrix() =>
        Matrix != null; // && matrix.IsUnityTransform() == false;

    /// <summary>
    ///     Determines whether [has color transform].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [has color transform]; otherwise, <c>false</c>.
    /// </returns>
    private bool HasColorTransform() =>
        ColorTransform != null;

    /// <summary>
    ///     Determines whether [has clip actions].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [has clip actions]; otherwise, <c>false</c>.
    /// </returns>
    private bool HasClipActions() =>
        clipActions != null &&
        clipActions.Length > 0 &&
        actionHead != null &&
        actionHead.Length > 0;

    /// <summary>
    ///     Determines whether this instance has character.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this instance has character; otherwise, <c>false</c>.
    /// </returns>
    private bool HasCharacter() =>
        TargetCharacterId > 0;

    /// <summary>
    ///     Determines whether [has clip depth].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [has clip depth]; otherwise, <c>false</c>.
    /// </returns>
    private bool HasClipDepth() =>
        clipDepth > 0;

    /// <summary>
    ///     Determines whether this instance has name.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this instance has name; otherwise, <c>false</c>.
    /// </returns>
    private bool HasName() =>
        name != null && name.Length > 0;

    /// <summary>
    ///     Determines whether this instance has ratio.
    /// </summary>
    /// <returns>
    ///     <c>true</c> if this instance has ratio; otherwise, <c>false</c>.
    /// </returns>
    private bool HasRatio() =>
        Ratio > 0.0;

    #endregion
}