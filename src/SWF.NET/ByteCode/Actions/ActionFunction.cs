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
using System.Collections;
using System.IO;
using SWF.NET.Utils;

namespace SWF.NET.ByteCode.Actions;

/// <summary>
///     bytecode instruction object
/// </summary>
public class ActionDefineFunction : MultiByteAction
{
	/// <summary>
	///     inner actions (function body)
	/// </summary>
	public ArrayList ActionRecord; // inner actions, function body

	/// <summary>
	///     function name
	/// </summary>
	public string Name;

	/// <summary>
	///     list of function parameters
	/// </summary>
	public string[] ParameterList;

    private int innerByteCount
    {
        get
        {
            var count = 0;
            for (var i = 0; i < ActionRecord.Count; i++)
            {
                var a = (BaseAction)ActionRecord[i];
                count += a.ByteCount;
            }

            return count;
        }
    }

    /// <see cref="BaseAction.ByteCount" />
    public override int ByteCount
    {
        get
        {
            var count = 3 + Name.Length + 1 + 2 + 2;
            for (var i = 0; i < ParameterList.Length; i++) count += ParameterList[i].Length + 1;
            count += innerByteCount;
            return count;
        }
    }

    /// <see cref="BaseAction.PopCount" />
    public override int PopCount =>
        0;

    /// <see cref="BaseAction.PushCount" />
    public override int PushCount =>
        1;

    /// <summary>
    ///     constructor
    /// </summary>
    /// <param name="n">function name</param>
    /// <param name="parmList">funtion parameters</param>
    /// <param name="actionRec">inner action block (function body)</param>
    public ActionDefineFunction(string n, string[] parmList, ArrayList actionRec) :
        base(ActionCode.DefineFunction)
    {
        Name = n;
        ParameterList = parmList;
        ActionRecord = actionRec;
    }

    /// <see cref="BaseAction.Compile" />
    public override void Compile(BinaryWriter w)
    {
        w.Write(Convert.ToByte(Code));
        w.Write(Convert.ToUInt16(ByteCount - innerByteCount - 3));
        BinaryStringRW.WriteString(w, Name);

        w.Write(Convert.ToUInt16(ParameterList.Length));
        foreach (var str in ParameterList) BinaryStringRW.WriteString(w, str);

        w.Write(Convert.ToUInt16(innerByteCount));
        foreach (var a in ActionRecord)
        {
            var action = (BaseAction)a;
            action.Compile(w);
        }
    }

    /// <summary>overriden ToString method</summary>
    public override string ToString()
    {
        var s = new string[ActionRecord.Count];
        for (var i = 0; i < ActionRecord.Count; i++)
        {
            var a = (BaseAction)ActionRecord[i];
            s[i] = a.ToString();
        }

        return string.Format(
            "function '{0}' ({1})\n{2}\nend function {3}",
            Name,
            string.Join(",", ParameterList),
            string.Join("\n", s),
            Name
        );
    }
}