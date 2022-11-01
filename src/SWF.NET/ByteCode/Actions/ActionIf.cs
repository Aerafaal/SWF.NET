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

namespace SWF.NET.ByteCode.Actions;

/// <summary>
///     bytecode instruction object
/// </summary>
public class ActionIf : MultiByteAction, IJump
{
    // branch offset

    /// <summary>
    ///     branch offset
    /// </summary>
    public int Offset { get; set; }

    /// <summary>
    ///     label id
    /// </summary>
    public int LabelId { get; set; }

    /// <see cref="BaseAction.ByteCount" />
    public override int ByteCount =>
        5;

    /// <see cref="BaseAction.PopCount" />
    public override int PopCount =>
        1;

    /// <see cref="BaseAction.PushCount" />
    public override int PushCount =>
        0;

    /// <summary>
    ///     constructor
    /// </summary>
    /// <param name="offs">branch offset</param>
    public ActionIf(int offs) : base(ActionCode.If)
    {
        Offset = offs;
        LabelId = 666;
    }

    /// <see cref="BaseAction.Compile" />
    public override void Compile(BinaryWriter w)
    {
        base.Compile(w);
        w.Write(Convert.ToInt16(Offset));
    }

    /// <summary>overriden ToString method</summary>
    public override string ToString() =>
        string.Format("branchIf {0} ({1})", LabelId, Offset);
}