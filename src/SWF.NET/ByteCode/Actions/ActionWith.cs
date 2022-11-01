﻿/*
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
public class ActionWith : MultiByteAction
{
    private ushort blockLength;

    /// <summary>
    ///     block length of actions enclosed by 'with'
    /// </summary>
    public int BlockLength
    {
        get => Convert.ToInt32(blockLength);
        set => blockLength = Convert.ToUInt16(value);
    }

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
    /// <param name="blockLen">block length of enclosed action</param>
    public ActionWith(ushort blockLen) : base(ActionCode.With) =>
        blockLength = blockLen;

    /// <summary>overriden ToString method</summary>
    public override string ToString() =>
        string.Format("with [{0}]", blockLength.ToString());

    /// <see cref="BaseAction.Compile" />
    public override void Compile(BinaryWriter w)
    {
        base.Compile(w);
        w.Write(blockLength);
    }
}