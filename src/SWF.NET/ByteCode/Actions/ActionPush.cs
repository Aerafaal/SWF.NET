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
using SWF.NET.Utils;

namespace SWF.NET.ByteCode.Actions;

/// <summary>
///     bytecode instruction object
/// </summary>
public class ActionPush : MultiByteAction
{
	/// <summary>
	///     enumaration of push types
	/// </summary>
	public enum PushType
    {
        /// <summary>push type 0: string</summary>
        String = 0,

        /// <summary>push type 1: float</summary>
        Float = 1,

        /// <summary>push type 2: null</summary>
        Null = 2,

        /// <summary>push type 3: undef</summary>
        Undef = 3,

        /// <summary>push type 4: register</summary>
        Register = 4,

        /// <summary>push type 5: bool</summary>
        Boolean = 5,

        /// <summary>push type 6: double</summary>
        Double = 6,

        /// <summary>push type 7: int</summary>
        Int = 7,

        /// <summary>push type 8: constant8</summary>
        Constant8 = 8,

        /// <summary>push type 9: constant9</summary>
        Constant16 = 9
    }

    private readonly string[] PushTypeNames = new string[10] { "string ", "float ", "null ", "undef ", "register ", "bool ", "double ", "int ", "var ", "var " };

    /// <summary>
    ///     push type
    /// </summary>
    public int Type;

    /// <summary>
    ///     push value
    /// </summary>
    public object Value;

    /// <see cref="BaseAction.ByteCount " />
    public override int ByteCount
    {
        get
        {
            switch ((PushType)Type)
            {
                case PushType.String:
                    var str = (string)Value;
                    return str.Length + 5;

                case PushType.Float:
                    return 8;

                case PushType.Register:
                    return 5;

                case PushType.Boolean:
                    return 5;

                case PushType.Double:
                    return 12;

                case PushType.Int:
                    return 8;

                case PushType.Constant8:
                    return 5;

                case PushType.Constant16:
                    return 6;
            }

            // Null, Undef
            return 4;
        }
    }

    /// <see cref="BaseAction.PopCount" />
    public override int PopCount =>
        0;

    /// <see cref="BaseAction.PushCount" />
    public override int PushCount =>
        1;

    /// <summary>
    ///     constructor.
    /// </summary>
    /// <param name="type">push type</param>
    /// <param name="val">push value</param>
    public ActionPush(int type, object val) : base(ActionCode.Push)
    {
        if (type < (int)PushType.String || type > (int)PushType.Constant16) throw new InvalidPushTypeException();
        Type = type;
        Value = val;
    }

    /// <summary>
    ///     get push value as int
    /// </summary>
    /// <returns>pushed value as int</returns>
    public int GetIntValue()
    {
        switch ((PushType)Type)
        {
            case PushType.Float: return Convert.ToInt32((float)Value);

            case PushType.Double: return Convert.ToInt32((double)Value);

            case PushType.String: return Convert.ToInt32((string)Value);

            case PushType.Int: return (int)Value;

            default:
                Console.WriteLine("WARNING");
                return -1;
        }
    }

    /// <summary>
    ///     get value as string
    /// </summary>
    /// <returns>pushed value as string</returns>
    public string GetStringValue() =>
        (PushType)Type == PushType.String ? (string)Value : null;

    /// <summary>
    ///     compile push type and value (but not action code), so method can
    ///     be used by <see cref="ActionPushList">ActionPushList</see> as well
    /// </summary>
    public void CompileBody(BinaryWriter w)
    {
        w.Write(Convert.ToByte(Type));

        switch ((PushType)Type)
        {
            case PushType.String:
                var stringToWrite = (string)Value;
                BinaryStringRW.WriteString(w, stringToWrite);
                break;

            case PushType.Float:
                w.Write(Convert.ToSingle(Value));
                break;

            case PushType.Register:
                w.Write(Convert.ToByte(Value));
                break;

            case PushType.Boolean:
                w.Write((bool)Value);
                break;

            case PushType.Double:
                var b = BitConverter.GetBytes((double)Value);
                for (var i = 0; i < 4; i++)
                {
                    var temp = b[i];
                    b[i] = b[4 + i];
                    b[4 + i] = temp;
                }

                w.Write(b);
                break;

            case PushType.Int:
                w.Write((int)Value);
                break;

            case PushType.Constant8:
                w.Write(Convert.ToByte(Value));
                break;

            case PushType.Constant16:
                w.Write(Convert.ToUInt16(Value));
                break;
        }
    }

    /// <see cref="BaseAction.Compile" />
    public override void Compile(BinaryWriter w)
    {
        base.Compile(w);
        CompileBody(w);
    }

    /// <summary>overriden ToString method</summary>
    public override string ToString()
    {
        var b = (PushType)Type == PushType.String ? "'" : "";
        return string.Format("push {1} as {0}", PushTypeNames[Type], b + Value + b);
    }
}