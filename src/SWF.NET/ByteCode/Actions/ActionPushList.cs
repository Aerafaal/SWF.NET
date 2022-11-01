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
public class ActionPushList : MultiByteAction
{
    private readonly ActionPush[] pushList;

    /// <summary>
    ///     length of push list
    /// </summary>
    public int Length =>
        pushList.Length;

    /// <summary>
    ///     indexer to access single push actions
    /// </summary>
    public ActionPush this[int i] =>
        pushList[i];

    /// <see cref="BaseAction.ByteCount" />
    public override int ByteCount
    {
        get
        {
            var count = 3;
            foreach (var p in pushList) count += p.ByteCount - 3;
            return count;
        }
    }

    /// <see cref="BaseAction.PopCount" />
    public override int PopCount =>
        0;

    /// <see cref="BaseAction.PushCount" />
    public override int PushCount =>
        pushList.Length;

    /// <summary>
    ///     constructor
    /// </summary>
    /// <param name="p">list of single push instructions</param>
    public ActionPushList(ActionPush[] p) : base(ActionCode.PushList) =>
        pushList = p;

    /// <see cref="BaseAction.Compile" />
    public override void Compile(BinaryWriter w)
    {
        // base.Compile(w);	
        w.Write((byte)0x96);
        w.Write(Convert.ToUInt16(ByteCount - 3));

        foreach (var push in pushList) push.CompileBody(w);
    }

    /// <summary>overriden ToString method</summary>
    public override string ToString()
    {
        var s = new string[pushList.Length];
        for (var i = 0; i < pushList.Length; i++) s[i] = pushList[i].ToString().Substring(5);
        return string.Format("push {0}", string.Join(", ", s));
    }
}