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
///     DefineMorphShapeTag defines a shape that will morph
///     from one form into another.
/// </summary>
/// <remarks>
///     <p>
///         Only the start and end shapes are defined. The Flash Player
///         will perform the interpolation that transforms the shape
///         at each staging in the morphing process.
///     </p>
///     <p>
///         Morphing can be applied to any shape, however there are
///         a few restrictions:
///         <ul>
///             <li>
///                 The start and end shapes must have the same number of
///                 edges (StraightEdgeRecord and CurvedEdgeRecord objects).
///             </li>
///             <li>
///                 The fill style (Solid, Bitmap or Gradient) must be the
///                 same in the start and end shape.
///             </li>
///             <li>
///                 If a bitmap fill style is used then the same image must
///                 be used in the start and end shapes.
///             </li>
///             <li>
///                 If a gradient fill style is used then the gradient must
///                 contain the same number of points in the start and end
///                 shape.
///             </li>
///             <li>
///                 The start and end shape must contain the same set of
///                 ShapeStyle objects.
///             </li>
///         </ul>
///     </p>
///     <p>
///         To perform the morphing of a shape the shape is placed in
///         the display list using a PlaceObject2Tag object. The ratio
///         attribute in the PlaceObject2Tag object defines the progress
///         of the morphing process. The ratio ranges between 0.0 and
///         1.0 where 0 represents the start of the morphing process
///         and 1.0, the end.
///     </p>
///     <p>
///         The edges may change their type when a shape is morphed.
///         Straight edges can become curves and vice versa.
///     </p>
///     <p>
///         This tag was introduced in Flash 3.
///     </p>
/// </remarks>
public class DefineMorphShapeTag : BaseTag, DefineTag
{
    #region Members

    private uint offset;

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="DefineMorphShapeTag" /> instance.
    /// </summary>
    public DefineMorphShapeTag() =>
        _tagCode = (int)TagCodeEnum.DefineMorphShape;

    /// <summary>
    ///     Creates a new <see cref="DefineMorphShapeTag" /> instance.
    /// </summary>
    /// <param name="characterId">Character id.</param>
    /// <param name="startBounds">Start bounds.</param>
    /// <param name="endBounds">End bounds.</param>
    /// <param name="offset">Offset.</param>
    /// <param name="morphFillStyles">Morph fill styles.</param>
    /// <param name="morphLineStyles">Morph line styles.</param>
    /// <param name="startEdges">Start edges.</param>
    /// <param name="endEdges">End edges.</param>
    public DefineMorphShapeTag(
        ushort characterId,
        Rect startBounds,
        Rect endBounds,
        uint offset,
        MorphFillStyleCollection morphFillStyles,
        MorphLineStyleCollection morphLineStyles,
        ShapeRecordCollection startEdges,
        ShapeRecordCollection endEdges)
    {
        this.CharacterId = characterId;
        this.StartBounds = startBounds;
        this.EndBounds = endBounds;
        this.offset = offset;
        this.MorphFillStyles = morphFillStyles;
        this.MorphLineStyles = morphLineStyles;
        this.StartEdges = startEdges;
        this.EndEdges = endEdges;

        _tagCode = (int)TagCodeEnum.DefineMorphShape;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     see <see cref="DefineTag" />
    /// </summary>
    public ushort CharacterId { get; set; }

    /// <summary>
    ///     Gets or sets the start bounds.
    /// </summary>
    public Rect StartBounds { get; set; }

    /// <summary>
    ///     Gets or sets the end bounds.
    /// </summary>
    public Rect EndBounds { get; set; }

    /// <summary>
    ///     Gets or sets the morph fill styles.
    /// </summary>
    public MorphFillStyleCollection MorphFillStyles { get; set; }

    /// <summary>
    ///     Gets or sets the morph line styles.
    /// </summary>
    public MorphLineStyleCollection MorphLineStyles { get; set; }

    /// <summary>
    ///     Gets or sets the start edges.
    /// </summary>
    public ShapeRecordCollection StartEdges { get; set; }

    /// <summary>
    ///     Gets or sets the end edges.
    /// </summary>
    public ShapeRecordCollection EndEdges { get; set; }

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
        binaryReader.SynchBits();

        StartBounds = new Rect();
        StartBounds.ReadData(binaryReader);

        binaryReader.SynchBits();
        EndBounds = new Rect();
        EndBounds.ReadData(binaryReader);
        binaryReader.SynchBits();

        offset = binaryReader.ReadUInt32();

        MorphFillStyles = new MorphFillStyleCollection();
        MorphFillStyles.ReadData(binaryReader);

        MorphLineStyles = new MorphLineStyleCollection();
        MorphLineStyles.ReadData(binaryReader);

        ShapeWithStyle.NumFillBits = (uint)MorphFillStyles.Count;
        ShapeWithStyle.NumLineBits = (uint)MorphLineStyles.Count;

        StartEdges = new ShapeRecordCollection();
        StartEdges.ReadData(binaryReader, ShapeType.None);

        ShapeWithStyle.NumFillBits = (uint)MorphFillStyles.Count;
        ShapeWithStyle.NumLineBits = (uint)MorphLineStyles.Count;

        EndEdges = new ShapeRecordCollection();
        EndEdges.ReadData(binaryReader, ShapeType.None);
    }

    /// <summary>
    ///     Gets the size of.
    /// </summary>
    /// <returns>Size of this object</returns>
    protected int GetSizeOf()
    {
        var res = 6;
        if (StartBounds != null)
            res += StartBounds.GetSizeOf();
        if (EndBounds != null)
            res += EndBounds.GetSizeOf();
        if (MorphFillStyles != null)
            res += MorphFillStyles.GetSizeOf();
        if (MorphLineStyles != null)
            res += MorphLineStyles.GetSizeOf();

        ShapeWithStyle.NumFillBits = (uint)MorphFillStyles.Count;
        ShapeWithStyle.NumLineBits = (uint)MorphLineStyles.Count;

        if (StartEdges != null)
            res += StartEdges.GetSizeOf();
        if (EndEdges != null)
            res += EndEdges.GetSizeOf();
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

        var rh = new RecordHeader(TagCode, GetSizeOf());

        rh.WriteTo(w);
        w.Write(CharacterId);

        w.SynchBits();
        if (StartBounds != null)
            StartBounds.WriteTo(w);
        w.SynchBits();
        if (EndBounds != null)
            EndBounds.WriteTo(w);

        w.Write(offset);
        if (MorphFillStyles != null)
            MorphFillStyles.WriteTo(w);
        if (MorphLineStyles != null)
            MorphLineStyles.WriteTo(w);

        ShapeWithStyle.NumFillBits = (uint)MorphFillStyles.Count;
        ShapeWithStyle.NumLineBits = (uint)MorphLineStyles.Count;

        if (StartEdges != null)
            StartEdges.WriteTo(w);
        if (EndEdges != null)
            EndEdges.WriteTo(w);

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
        writer.WriteStartElement("DefineMorphShapeTag");

        writer.WriteAttributeString("CharacterId", CharacterId.ToString());
        if (StartBounds != null)
            StartBounds.Serialize(writer);
        if (EndBounds != null)
            EndBounds.Serialize(writer);
        writer.WriteElementString("Offset", offset.ToString());
        if (MorphFillStyles != null)
            MorphFillStyles.Serialize(writer);
        if (MorphLineStyles != null)
            MorphLineStyles.Serialize(writer);

        ShapeWithStyle.NumFillBits = (uint)MorphFillStyles.Count;
        ShapeWithStyle.NumLineBits = (uint)MorphLineStyles.Count;

        if (StartEdges != null)
            StartEdges.Serialize(writer);
        if (EndEdges != null)
            EndEdges.Serialize(writer);

        writer.WriteEndElement();
    }

    #endregion
}