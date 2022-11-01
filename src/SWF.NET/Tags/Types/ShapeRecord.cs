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

using System;
using System.Xml;
using SWF.NET.Utils;

namespace SWF.NET.Tags.Types;

/// <summary>
///     ShapeRecord class
/// </summary>
public abstract class ShapeRecord : SizeStruct, ISwfSerializer
{
    #region Members

    /// <summary>
    ///     Type of shape record, stored as a flag
    /// </summary>
    protected bool typeFlag;

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="ShapeRecord" /> instance.
    /// </summary>
    protected ShapeRecord()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="ShapeRecord" /> instance.
    /// </summary>
    /// <param name="typeFlag">Type flag.</param>
    protected ShapeRecord(bool typeFlag) =>
        this.typeFlag = typeFlag;

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the bit size of.
    /// </summary>
    /// <param name="currentLength">Length of the current.</param>
    /// <returns></returns>
    public virtual int GetBitSizeOf(int currentLength) =>
        1;

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public virtual void WriteTo(BufferedBinaryWriter writer) =>
        writer.WriteBoolean(typeFlag);

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public virtual void Serialize(XmlWriter writer)
    {
    }

    #endregion
}

/// <summary>
///     NonEdgeRecord class
/// </summary>
public abstract class NonEdgeRecord : ShapeRecord
{
    #region Protected Ctor

    /// <summary>
    ///     Creates a new <see cref="NonEdgeRecord" /> instance.
    /// </summary>
    protected NonEdgeRecord() : base(false)
    {
    }

    #endregion
}

/// <summary>
///     EndShapeRecord defines the end of a shape sequence.
/// </summary>
/// <remarks>
///     <p>
///         When this object is readed by the Flash Player, it stop to
///         draw the current shape records.
///     </p>
///     <p>
///         This tag was introduced in Flash 1.
///     </p>
/// </remarks>
public class EndShapeRecord : NonEdgeRecord
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="EndShapeRecord" /> instance.
    /// </summary>
    public EndShapeRecord()
    {
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the bit size of.
    /// </summary>
    /// <param name="currentLength">Length of the current.</param>
    /// <returns></returns>
    public override int GetBitSizeOf(int currentLength) =>
        5 + base.GetBitSizeOf(currentLength);

    /// <summary>
    ///     Writes to.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void WriteTo(BufferedBinaryWriter writer)
    {
        base.WriteTo(writer);
        writer.WriteUBits(0, 5);
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("EndShapeRecord");
        writer.WriteEndElement();
    }

    #endregion
}

/// <summary>
///     StyleChangeRecord is used to change the drawing environment
///     when a shape is drawn.
/// </summary>
/// <remarks>
///     <p>
///         Three operations can be performed:
///         <ul>
///             <li>Select a line style or fill style.</li>
///             <li>Move the current drawing point.</li>
///             <li>Define a new set of line and fill styles.</li>
///         </ul>
///     </p>
///     <p>
///         An StyleChangeRecord object can specify one or more of the operations
///         rather than specifying them in separate StyleChangeRecord objects - compacting
///         the size of the binary data when the object is encoded. Conversely if
///         an operation is not defined then the values may be omitted.
///     </p>
///     <p>
///         A new drawing point is specified using the absolute x and y coordinates.
///         If an StyleChangeRecord object is the first in a shape then the current
///         drawing point is the origin of the shape (0,0).
///     </p>
///     <p>
///         New fill and line styles can be added to the StyleChangeRecord object
///         to change the way shapes are drawn.
///     </p>
///     <p>
///         This was introduced in Flash 1.
///     </p>
/// </remarks>
/// <example>
/// </example>
public class StyleChangeRecord : NonEdgeRecord
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="StyleChangeRecord" /> instance.
    /// </summary>
    public StyleChangeRecord()
    {
    }

    /// <summary>
    ///     Creates a new <see cref="StyleChangeRecord" /> instance.
    /// </summary>
    /// <param name="moveX">Move X.</param>
    /// <param name="moveY">Move Y.</param>
    public StyleChangeRecord(int moveX, int moveY)
    {
        MoveDeltaX = moveX;
        MoveDeltaY = moveY;
    }

    /// <summary>
    ///     Creates a new <see cref="StyleChangeRecord" /> instance.
    /// </summary>
    /// <param name="moveX">Move X.</param>
    /// <param name="moveY">Move Y.</param>
    /// <param name="fillStyle1">Fill style1.</param>
    public StyleChangeRecord(int moveX, int moveY, ushort fillStyle1)
    {
        MoveDeltaX = moveX;
        MoveDeltaY = moveY;
        FillStyle1 = fillStyle1;
    }

    /// <summary>
    ///     Creates a new <see cref="StyleChangeRecord" /> instance.
    /// </summary>
    /// <param name="lineStyle">Line style.</param>
    /// <param name="fillStyle0">Fill style0.</param>
    /// <param name="fillStyle1">Fill style1.</param>
    public StyleChangeRecord(ushort lineStyle, ushort fillStyle0, ushort fillStyle1)
    {
        LineStyle = lineStyle;
        FillStyle0 = fillStyle0;
        FillStyle1 = fillStyle1;
    }

    /// <summary>
    ///     Creates a new <see cref="StyleChangeRecord" /> instance.
    /// </summary>
    /// <param name="fillStyles">Fill styles.</param>
    /// <param name="lineStyles">Line styles.</param>
    public StyleChangeRecord(FillStyleCollection fillStyles, LineStyleCollection lineStyles)
    {
        FillStyles = fillStyles;
        LineStyles = lineStyles;
    }

    /// <summary>
    ///     Creates a new <see cref="StyleChangeRecord" /> instance.
    /// </summary>
    /// <param name="lineStyle">Line style.</param>
    /// <param name="fillStyle0">Fill style0.</param>
    /// <param name="fillStyle1">Fill style1.</param>
    /// <param name="moveX">Move X.</param>
    /// <param name="moveY">Move Y.</param>
    public StyleChangeRecord(
        ushort lineStyle,
        ushort fillStyle0,
        ushort fillStyle1,
        int moveX,
        int moveY)
    {
        LineStyle = lineStyle;
        FillStyle0 = fillStyle0;
        FillStyle1 = fillStyle1;
        MoveDeltaX = moveX;
        MoveDeltaY = moveY;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the fill styles.
    /// </summary>
    /// <value></value>
    public FillStyleCollection FillStyles { get; set; }

    /// <summary>
    ///     Gets or sets the line styles.
    /// </summary>
    /// <value></value>
    public LineStyleCollection LineStyles { get; set; }

    /// <summary>
    ///     Gets or sets the line style.
    /// </summary>
    /// <value></value>
    public int LineStyle { get; set; } = int.MinValue;

    /// <summary>
    ///     Gets or sets the fill style1.
    /// </summary>
    /// <value></value>
    public int FillStyle1 { get; set; } = int.MinValue;

    /// <summary>
    ///     Gets or sets the fill style0.
    /// </summary>
    /// <value></value>
    public int FillStyle0 { get; set; } = int.MinValue;

    /// <summary>
    ///     Gets or sets the move delta X.
    /// </summary>
    /// <value></value>
    public int MoveDeltaX { get; set; } = int.MinValue;

    /// <summary>
    ///     Gets or sets the move delta Y.
    /// </summary>
    /// <value></value>
    public int MoveDeltaY { get; set; } = int.MinValue;

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    /// <param name="flags">Flags.</param>
    /// <param name="numFillBits">Num fill bits.</param>
    /// <param name="numLineBits">Num line bits.</param>
    /// <param name="shapeType">Shape type.</param>
    public void ReadData(
        BufferedBinaryReader binaryReader,
        byte flags,
        ref byte numFillBits,
        ref byte numLineBits,
        ShapeType shapeType)
    {
        SetStartPoint(binaryReader);
        var stateNewStyle = (flags & 0x10) != 0;
        var stateLineStyle = (flags & 0x08) != 0;
        var stateFillStyle1 = (flags & 0x04) != 0;
        var stateFillStyle0 = (flags & 0x02) != 0;
        var stateMoveTo = (flags & 0x01) != 0;

        if (stateMoveTo)
        {
            var bits = binaryReader.ReadUBits(5);
            MoveDeltaX = binaryReader.ReadSBits(bits);
            MoveDeltaY = binaryReader.ReadSBits(bits);
        }

        if (stateFillStyle0) FillStyle0 = (int)binaryReader.ReadUBits(numFillBits);
        if (stateFillStyle1) FillStyle1 = (int)binaryReader.ReadUBits(numFillBits);
        if (stateLineStyle) LineStyle = (int)binaryReader.ReadUBits(numLineBits);

        FillStyles = null;
        LineStyles = null;

        if (stateNewStyle)
        {
            FillStyles = new FillStyleCollection();
            FillStyles.ReadData(binaryReader, shapeType);
            LineStyles = new LineStyleCollection();
            LineStyles.ReadData(binaryReader, shapeType);

            numFillBits = (byte)binaryReader.ReadUBits(4);
            numLineBits = (byte)binaryReader.ReadUBits(4);
        }

        SetEndPoint(binaryReader);
    }

    /// <summary>
    ///     see <see cref="ShapeRecord.GetBitSizeOf" />
    /// </summary>
    public override int GetBitSizeOf(int currentLength)
    {
        var res = base.GetBitSizeOf(currentLength);
        res += 5;

        if (HasMoveTo())
        {
            var moveBitsNum = GetMoveNumBits();
            res += Convert.ToInt32(5 + 2 * moveBitsNum);
        }

        if (HasFillStyle0())
            res += (int)ShapeWithStyle.NumFillBits;
        if (HasFillStyle1())
            res += (int)ShapeWithStyle.NumFillBits;
        if (HasLineStyle())
            res += (int)ShapeWithStyle.NumLineBits;
        if (HasNewStyle())
        {
            if ((res + currentLength) % 8 != 0)
                res += 8 - (res + currentLength) % 8;
            ShapeWithStyle.NumFillBits = BufferedBinaryWriter.GetNumBits((uint)FillStyles.Count);
            ShapeWithStyle.NumLineBits = BufferedBinaryWriter.GetNumBits((uint)LineStyles.Count);
            res += FillStyles.GetSizeOf() * 8;
            res += LineStyles.GetSizeOf() * 8;
            res += 8;
        }

        return res;
    }

    /// <summary>
    ///     Determines whether [has new style].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [has new style]; otherwise, <c>false</c>.
    /// </returns>
    private bool HasNewStyle() =>
        (LineStyles != null && LineStyles.Count > 0) || (FillStyles != null && FillStyles.Count > 0);

    /// <summary>
    ///     Determines whether [has line style].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [has line style]; otherwise, <c>false</c>.
    /// </returns>
    private bool HasLineStyle() =>
        LineStyle != int.MinValue;

    /// <summary>
    ///     Determines whether [has fill style1].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [has fill style1]; otherwise, <c>false</c>.
    /// </returns>
    private bool HasFillStyle1() =>
        FillStyle1 != int.MinValue;

    /// <summary>
    ///     Determines whether [has fill style0].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [has fill style0]; otherwise, <c>false</c>.
    /// </returns>
    private bool HasFillStyle0() =>
        FillStyle0 != int.MinValue;

    /// <summary>
    ///     Determines whether [has move to].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [has move to]; otherwise, <c>false</c>.
    /// </returns>
    private bool HasMoveTo() =>
        MoveDeltaX != int.MinValue && MoveDeltaY != int.MinValue;

    /// <summary>
    ///     Writes to.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void WriteTo(BufferedBinaryWriter writer)
    {
        base.WriteTo(writer);
        var stateNewStyle = HasNewStyle();
        var stateLineStyle = HasLineStyle();
        var stateFillStyle0 = HasFillStyle0();
        var stateFillStyle1 = HasFillStyle1();
        var stateMoveTo = HasMoveTo();

        writer.WriteBoolean(stateNewStyle);
        writer.WriteBoolean(stateLineStyle);
        writer.WriteBoolean(stateFillStyle1);
        writer.WriteBoolean(stateFillStyle0);
        writer.WriteBoolean(stateMoveTo);

        if (stateMoveTo)
        {
            var moveBitsNum = GetMoveNumBits();
            writer.WriteUBits(moveBitsNum, 5);
            writer.WriteSBits(MoveDeltaX, moveBitsNum);
            writer.WriteSBits(MoveDeltaY, moveBitsNum);
        }

        if (stateFillStyle0) writer.WriteUBits((uint)FillStyle0, ShapeWithStyle.NumFillBits);
        if (stateFillStyle1) writer.WriteUBits((uint)FillStyle1, ShapeWithStyle.NumFillBits);
        if (stateLineStyle) writer.WriteUBits((uint)LineStyle, ShapeWithStyle.NumLineBits);

        if (stateNewStyle)
        {
            FillStyles.WriteTo(writer);
            LineStyles.WriteTo(writer);
            ShapeWithStyle.NumFillBits = BufferedBinaryWriter.GetNumBits((uint)FillStyles.Count);
            ShapeWithStyle.NumLineBits = BufferedBinaryWriter.GetNumBits((uint)LineStyles.Count);
            writer.WriteUBits(ShapeWithStyle.NumFillBits, 4);
            writer.WriteUBits(ShapeWithStyle.NumLineBits, 4);
        }
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("StyleChangeRecord");
        SerializeBinarySize(writer);
        writer.WriteElementString("StateNewStyle", HasNewStyle().ToString());
        ;
        writer.WriteElementString("StateLineStyle", HasLineStyle().ToString());
        writer.WriteElementString("StateFillStyle1", HasFillStyle1().ToString());
        writer.WriteElementString("StateFillStyle0", HasFillStyle0().ToString());
        writer.WriteElementString("StateMoveTo", HasMoveTo().ToString());
        if (HasMoveTo())
        {
            writer.WriteElementString("MoveDeltaX", MoveDeltaX.ToString());
            writer.WriteElementString("MoveDeltaY", MoveDeltaY.ToString());
        }

        if (HasFillStyle0()) writer.WriteElementString("FillStyle0", FillStyle0.ToString());
        if (HasFillStyle1()) writer.WriteElementString("FillStyle1", FillStyle1.ToString());
        if (HasLineStyle()) writer.WriteElementString("LineStyle", LineStyle.ToString());
        if (HasNewStyle())
        {
            FillStyles.Serialize(writer);
            LineStyles.Serialize(writer);
            ShapeWithStyle.NumFillBits = BufferedBinaryWriter.GetNumBits((uint)FillStyles.Count);
            ShapeWithStyle.NumLineBits = BufferedBinaryWriter.GetNumBits((uint)LineStyles.Count);
            writer.WriteElementString("NumFillBits", ShapeWithStyle.NumFillBits.ToString());
            writer.WriteElementString("NumLineBits", ShapeWithStyle.NumLineBits.ToString());
        }

        writer.WriteEndElement();
    }


    /// <summary>
    ///     Gets the move num bits.
    /// </summary>
    /// <returns></returns>
    private uint GetMoveNumBits()
    {
        uint res = 0;
        uint tmp = 0;
        tmp = BufferedBinaryWriter.GetNumBits(MoveDeltaX);
        if (tmp > res)
            res = tmp;
        tmp = BufferedBinaryWriter.GetNumBits(MoveDeltaY);
        if (tmp > res)
            res = tmp;
        return res;
    }

    #endregion
}

/// <summary>
///     EdgeRecord
/// </summary>
public abstract class EdgeRecord : ShapeRecord
{
    #region Members

    /// <summary>
    ///     Flag to know if the edge record is a straight record
    /// </summary>
    protected bool straightFlag;

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="EdgeRecord" /> instance.
    /// </summary>
    protected EdgeRecord() : base(true)
    {
    }

    /// <summary>
    ///     Creates a new <see cref="EdgeRecord" /> instance.
    /// </summary>
    /// <param name="straightFlag">Straight flag.</param>
    protected EdgeRecord(bool straightFlag) : base(true) =>
        this.straightFlag = straightFlag;

    #endregion

    #region Methods

    /// <summary>
    ///     see <see cref="ShapeRecord.GetBitSizeOf" />
    /// </summary>
    public override int GetBitSizeOf(int currentLength) =>
        1 + base.GetBitSizeOf(currentLength);

    /// <summary>
    ///     Writes to.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void WriteTo(BufferedBinaryWriter writer)
    {
        base.WriteTo(writer);
        writer.WriteBoolean(straightFlag);
    }

    #endregion
}

/// <summary>
///     StraightEdgeRecord defines a straight line.
/// </summary>
/// <remarks>
///     <p>
///         The line is drawn from the current drawing point to the end
///         point specified in the StraightEdgeRecord object which is specified
///         relative to the current drawing point. Once the line is drawn,
///         the end of the line is now the current drawing point.
///     </p>
///     <p>
///         The relative coordinates are specified in twips
///         (where 20 twips = 1 pixel) and must be in the range -65536..65535.
///     </p>
///     <p>
///         Lines are drawn with rounded corners and line ends. Different
///         join and line end styles can be created by drawing line segments
///         as a sequence of filled shapes. With 1 twip equal to 1/20th
///         of a pixel this technique can easily be used to draw the
///         narrowest of visible lines.
///     </p>
///     <p>
///         This tag was introduced in Flash 1.
///     </p>
/// </remarks>
public class StraightEdgeRecord : EdgeRecord
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="StraightEdgeRecord" /> instance.
    /// </summary>
    public StraightEdgeRecord() : base(true)
    {
    }

    /// <summary>
    ///     Creates a new <see cref="StraightEdgeRecord" /> instance.
    /// </summary>
    /// <param name="deltaX">Delta X.</param>
    /// <param name="deltaY">Delta Y.</param>
    public StraightEdgeRecord(int deltaX, int deltaY) : base(true)
    {
        DeltaX = deltaX;
        DeltaY = deltaY;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the delta Y.
    ///     That's y-coordinate of the end point of the line, relative
    ///     to the current drawing point.
    /// </summary>
    public int DeltaY { get; set; }

    /// <summary>
    ///     Gets or sets the delta X.
    ///     That's x-coordinate of the end point of the line, relative
    ///     to the current drawing point.
    /// </summary>
    public int DeltaX { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    /// <param name="flags">Flags.</param>
    public void ReadData(BufferedBinaryReader binaryReader, byte flags)
    {
        SetStartPoint(binaryReader);
        var numBits = (byte)(flags & 0x0F);
        var generalLineFLag = binaryReader.ReadBoolean();
        DeltaX = 0;
        DeltaY = 0;

        if (generalLineFLag)
        {
            DeltaX = binaryReader.ReadSBits((uint)(numBits + 2));
            DeltaY = binaryReader.ReadSBits((uint)(numBits + 2));
        }
        else
        {
            var vertLineFlag = binaryReader.ReadBoolean();
            if (!vertLineFlag)
                DeltaX = binaryReader.ReadSBits((uint)(numBits + 2));
            else
                DeltaY = binaryReader.ReadSBits((uint)(numBits + 2));
        }

        SetEndPoint(binaryReader);
    }

    /// <summary>
    ///     Gets the num bits.
    /// </summary>
    /// <returns></returns>
    private uint GetNumBits()
    {
        uint res = 0;
        uint tmp = 0;
        tmp = BufferedBinaryWriter.GetNumBits(DeltaX);
        if (tmp > res)
            res = tmp;
        tmp = BufferedBinaryWriter.GetNumBits(DeltaY);
        if (tmp > res)
            res = tmp;
        return res;
    }

    /// <summary>
    ///     Determines whether [has general line].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [has general line]; otherwise, <c>false</c>.
    /// </returns>
    private bool HasGeneralLine() =>
        DeltaX != 0 && DeltaY != 0;

    /// <summary>
    ///     Determines whether [has vertical line].
    /// </summary>
    /// <returns>
    ///     <c>true</c> if [has vertical line]; otherwise, <c>false</c>.
    /// </returns>
    private bool HasVerticalLine() =>
        DeltaX == 0;

    /// <summary>
    ///     see <see cref="ShapeRecord.GetBitSizeOf" />
    /// </summary>
    public override int GetBitSizeOf(int currentLength)
    {
        var res = base.GetBitSizeOf(currentLength);
        res += 5; //numbits + generalLineFlag

        var numBits = GetNumBits();
        if (HasGeneralLine())
            res += Convert.ToInt32(numBits * 2);
        else
        {
            res++;
            res += Convert.ToInt32(numBits);
        }

        return res;
    }

    /// <summary>
    ///     Writes to.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void WriteTo(BufferedBinaryWriter writer)
    {
        base.WriteTo(writer);
        var numBits = GetNumBits();
        writer.WriteUBits(numBits - 2, 4);

        var generalLineFlag = HasGeneralLine();
        writer.WriteBoolean(generalLineFlag);

        if (generalLineFlag)
        {
            writer.WriteSBits(DeltaX, numBits);
            writer.WriteSBits(DeltaY, numBits);
        }
        else
        {
            var vertLineFlag = HasVerticalLine();
            writer.WriteBoolean(vertLineFlag);
            if (!vertLineFlag)
                writer.WriteSBits(DeltaX, numBits);
            else
                writer.WriteSBits(DeltaY, numBits);
        }
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("StraightEdgeRecord");
        SerializeBinarySize(writer);
        var generalLineFlag = HasGeneralLine();
        if (generalLineFlag)
        {
            writer.WriteElementString("DeltaX", DeltaX.ToString());
            writer.WriteElementString("DeltaY", DeltaY.ToString());
        }
        else
        {
            var vertLineFlag = HasVerticalLine();
            if (!vertLineFlag)
                writer.WriteElementString("DeltaX", DeltaX.ToString());
            else
                writer.WriteElementString("DeltaY", DeltaY.ToString());
        }

        writer.WriteEndElement();
    }

    #endregion
}

/// <summary>
///     CurvedEdgeRecord is used to define a curve.
///     Curved lines are constructed using a Quadratic Bezier curve.
/// </summary>
/// <remarks>
///     <p>
///         The curve is specified using two points relative to the current
///         drawing position, an off-curve control point and an on-curve anchor
///         point which defines the end-point of the curve.
///     </p>
///     <p>
///         To define a curve the points are defined as pairs of relative coordinates.
///         The control point is specified relative to the current drawing point
///         and the anchor point is specified relative to the control point.
///         Once the line is drawn, the anchor point becomes the current drawing
///         point.
///     </p>
///     <p>
///         The relative coordinates are specified in twips (where 20 twips = 1 pixel)
///         and must be in the range -65536..65535.
///     </p>
///     <p>
///         The CurvedEdge record was introduced in Flash 1.
///     </p>
/// </remarks>
public class CurvedEdgeRecord : EdgeRecord
{
    #region Members

    #endregion

    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="CurvedEdgeRecord" /> instance.
    /// </summary>
    public CurvedEdgeRecord() : base(false)
    {
    }

    /// <summary>
    ///     Creates a new <see cref="CurvedEdgeRecord" /> instance.
    /// </summary>
    /// <param name="controlDeltaX">The x-coordinate of the control point relative to the current drawing point.</param>
    /// <param name="controlDeltaY">The y-coordinate of the control point relative to the current drawing point.</param>
    /// <param name="anchorDeltaX">The x-coordinate of the anchor point relative to the control point.</param>
    /// <param name="anchorDeltaY">The y-coordinate of the anchor point relative to the control point.</param>
    public CurvedEdgeRecord(
        int controlDeltaX,
        int controlDeltaY,
        int anchorDeltaX,
        int anchorDeltaY)
        : base(false)
    {
        ControlDeltaX = controlDeltaX;
        ControlDeltaY = controlDeltaY;
        AnchorDeltaX = anchorDeltaX;
        AnchorDeltaY = anchorDeltaY;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the anchor delta Y.
    ///     This is the y-coordinate of the anchor point relative
    ///     to the control point.
    /// </summary>
    public int AnchorDeltaY { get; set; }

    /// <summary>
    ///     Gets or sets the anchor delta X.
    ///     This is the x-coordinate of the anchor point relative
    ///     to the control point.
    /// </summary>
    public int AnchorDeltaX { get; set; }

    /// <summary>
    ///     Gets or sets the control delta X.
    ///     This is the x-coordinate of the control point relative
    ///     to the current drawing point.
    /// </summary>
    public int ControlDeltaX { get; set; }

    /// <summary>
    ///     Gets or sets the control delta Y.
    ///     This is the y-coordinate of the control point relative
    ///     to the current drawing point.
    /// </summary>
    public int ControlDeltaY { get; set; }

    #endregion

    #region Methods

    /// <summary>
    ///     Reads the data.
    /// </summary>
    /// <param name="binaryReader">Binary reader.</param>
    /// <param name="flags">Flags.</param>
    public void ReadData(BufferedBinaryReader binaryReader, byte flags)
    {
        SetStartPoint(binaryReader);
        var numBits = (byte)(flags & 0x0F);

        ControlDeltaX = binaryReader.ReadSBits((uint)(numBits + 2));
        ControlDeltaY = binaryReader.ReadSBits((uint)(numBits + 2));
        AnchorDeltaX = binaryReader.ReadSBits((uint)(numBits + 2));
        AnchorDeltaY = binaryReader.ReadSBits((uint)(numBits + 2));
        SetEndPoint(binaryReader);
    }

    /// <summary>
    ///     Gets the num bits.
    /// </summary>
    /// <returns></returns>
    private uint GetNumBits()
    {
        uint res = 0;
        uint tmp = 0;

        tmp = BufferedBinaryWriter.GetNumBits(ControlDeltaX);
        if (tmp > res)
            res = tmp;
        tmp = BufferedBinaryWriter.GetNumBits(ControlDeltaY);
        if (tmp > res)
            res = tmp;
        tmp = BufferedBinaryWriter.GetNumBits(AnchorDeltaX);
        if (tmp > res)
            res = tmp;
        tmp = BufferedBinaryWriter.GetNumBits(AnchorDeltaY);
        if (tmp > res)
            res = tmp;
        return res;
    }

    /// <summary>
    ///     see <see cref="ShapeRecord.GetBitSizeOf" />
    /// </summary>
    public override int GetBitSizeOf(int currentLength)
    {
        var res = base.GetBitSizeOf(currentLength);
        res += 4;
        res += Convert.ToInt32(GetNumBits() * 4);
        return res;
    }

    /// <summary>
    ///     Writes to a binary writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void WriteTo(BufferedBinaryWriter writer)
    {
        base.WriteTo(writer);
        var numBits = GetNumBits();
        writer.WriteUBits(numBits - 2, 4);
        writer.WriteSBits(ControlDeltaX, numBits);
        writer.WriteSBits(ControlDeltaY, numBits);
        writer.WriteSBits(AnchorDeltaX, numBits);
        writer.WriteSBits(AnchorDeltaY, numBits);
    }

    /// <summary>
    ///     Serializes the specified writer.
    /// </summary>
    /// <param name="writer">Writer.</param>
    public override void Serialize(XmlWriter writer)
    {
        writer.WriteStartElement("CurvedEdgeRecord");
        SerializeBinarySize(writer);
        writer.WriteElementString("ControlDeltaX", ControlDeltaX.ToString());
        writer.WriteElementString("ControlDeltaY", ControlDeltaY.ToString());
        writer.WriteElementString("AnchorDeltaX", AnchorDeltaX.ToString());
        writer.WriteElementString("AnchorDeltaY", AnchorDeltaY.ToString());
        writer.WriteEndElement();
    }

    #endregion
}