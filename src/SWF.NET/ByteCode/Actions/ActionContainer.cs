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

namespace SWF.NET.ByteCode.Actions;

/// <summary>
///     pseudo/helper action for aggregating a whole list of actions into one action.
/// </summary>
public class ActionContainer : BaseAction
{
    private readonly int byteCount;
    private readonly int popCount;
    private readonly int pushCount;

    /// <summary>list of contained actions</summary>
    public BaseAction[] ActionList { get; }

    /// <see cref="BaseAction.ByteCount" />
    public override int ByteCount =>
        byteCount;

    /// <see cref="BaseAction.PushCount" />
    public override int PushCount
    {
        get => pushCount;
        set { }
    }

    /// <see cref="BaseAction.PopCount" />
    public override int PopCount
    {
        get => popCount;
        set { }
    }

    /// <summary>constructor</summary>
    public ActionContainer(BaseAction[] aList) : base(ActionCode.Container)
    {
        ActionList = aList;
        byteCount = 0;
        pushCount = 0;
        popCount = 0;
        foreach (var a in aList)
        {
            byteCount += a.ByteCount;
            pushCount += a.PushCount;
            popCount += a.PopCount;
        }
    }

    /// <see cref="BaseAction.Compile" />
    public override void Compile(BinaryWriter w)
    {
        for (var i = 0; i < ActionList.Length; i++)
        {
            var action = ActionList[i];
            action.Compile(w);
        }
    }
}