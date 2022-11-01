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
using System.Collections;
using System.IO;
using SWF.NET.Utils;

namespace SWF.NET.ByteCode.Actions;

/// <summary>
///     bytecode instruction object
/// </summary>
public class ActionConstantPool : MultiByteAction
{
	/// <summary>
	///     list of constant pool strings by index
	/// </summary>
	public ArrayList ConstantList;

	/// <see cref="BaseAction.ByteCount" />
	public override int ByteCount
    {
        get
        {
            var count = 5;
            for (var i = 0; i < ConstantList.Count; i++)
            {
                var s = (string)ConstantList[i];
                count += s.Length + 1;
            }

            return count;
        }
    }

	/// <summary>
	///     constructor
	/// </summary>
	/// <param name="c">constant list</param>
	public ActionConstantPool(string[] c) : base(ActionCode.ConstantPool) =>
        ConstantList = new ArrayList(c);

	/// <summary>
	///     overriden ToString() method
	/// </summary>
	/// ///
	/// <summary>overriden ToString method</summary>
	public override string ToString()
    {
        var clist = new string[ConstantList.Count];
        for (var i = 0; i < clist.Length; i++) clist[i] = string.Format("'{0}'", ConstantList[i]);
        return string.Format("constantPool {0}", string.Join(",", clist));
    }

	/// <see cref="BaseAction.Compile" />
	/// ///
	/// <see cref="BaseAction.Compile" />
	public override void Compile(BinaryWriter w)
    {
        base.Compile(w);
        w.Write(Convert.ToUInt16(ConstantList.Count));

        foreach (var c in ConstantList)
        {
            var stringToWrite = (string)c;
            BinaryStringRW.WriteString(w, stringToWrite);
        }
    }
}