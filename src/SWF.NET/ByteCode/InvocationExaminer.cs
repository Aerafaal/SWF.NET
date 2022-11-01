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
using SWF.NET.ByteCode.Actions;

namespace SWF.NET.ByteCode;

/// <summary>
///     The InvocationExaminer class analyses method/function calls and object
///     initializations to find out how many values get pushed on or popped from stack.
///     It is passed to an instance of CodeTraverser by the decompiler.
/// </summary>
public class InvocationExaminer : IActionExaminer
{
    private readonly Stack stack;

    /// <summary>
    ///     Constructor.
    /// </summary>
    public InvocationExaminer()
    {
        stack = new Stack();

        // we need to put some dummy values on stack
        // in case values get popped from empty stack
        for (var i = 0; i < 5; i++) stack.Push(null);
    }

    /// <summary>
    ///     Private constructor, used by Clone method.
    /// </summary>
    private InvocationExaminer(Stack s) =>
        stack = s;

    /// <summary>
    ///     Clone method, necessary for handling branches.
    /// </summary>
    public IActionExaminer Clone() =>
        new InvocationExaminer((Stack)stack.Clone());

    /// <summary>
    ///     Examine byte code action at index in action record.
    /// </summary>
    public void Examine(int index, BaseAction a)
    {
        ActionPush p;
        int args;
        CodeTraverser trav;

        switch (a.Code)
        {
            case (int)ActionCode.StackSwap:
                var o1 = stack.Pop();
                var o2 = stack.Pop();
                stack.Push(o1);
                stack.Push(o2);
                break;

            case (int)ActionCode.PushDuplicate:
                stack.Push(stack.Peek());
                break;

            case (int)ActionCode.Push:
                stack.Push(a);
                break;

            // --------------------------------------

            case (int)ActionCode.CallMethod:

                var cm = a as ActionCallMethod;

                stack.Pop(); // name
                stack.Pop(); // script object

                p = stack.Pop() as ActionPush;
                args = p.GetIntValue();
                for (var i = 0; i < args; i++) stack.Pop();
                if (args > -1) cm.NumArgs = args;
                stack.Push(null);
                break;

            case (int)ActionCode.CallFunction:

                var cf = a as ActionCallFunction;
                stack.Pop(); // name					
                p = stack.Pop() as ActionPush;
                args = p.GetIntValue();
                for (var i = 0; i < args; i++) stack.Pop();
                if (args > -1) cf.NumArgs = args;
                stack.Push(null);
                break;

            // --------------------------------------

            case (int)ActionCode.InitArray:

                var ia = a as ActionInitArray;
                p = stack.Pop() as ActionPush;
                args = p.GetIntValue();
                for (var i = 0; i < args; i++) stack.Pop();

                ia.NumValues = args;
                stack.Push(null);
                break;

            case (int)ActionCode.InitObject:

                var io = a as ActionInitObject;
                p = stack.Pop() as ActionPush;
                args = p.GetIntValue();
                for (var i = 0; i < args; i++)
                {
                    stack.Pop();
                    stack.Pop();
                }

                io.NumProps = args;
                stack.Push(null);
                break;

            case (int)ActionCode.NewObject:

                var n = a as ActionNewObject;
                stack.Pop();
                p = stack.Pop() as ActionPush;
                args = p.GetIntValue();
                for (var i = 0; i < args; i++) stack.Pop();
                n.NumArgs = args;
                stack.Push(null);
                break;

            case (int)ActionCode.NewMethod:
                var nm = a as ActionNewMethod;
                stack.Pop();
                stack.Pop();
                p = stack.Pop() as ActionPush;
                args = p.GetIntValue();
                for (var i = 0; i < args; i++) stack.Pop();
                nm.NumArgs = args;
                stack.Push(null);
                break;

            case (int)ActionCode.Implements:

                var aimpl = a as ActionImplements;
                stack.Pop(); // constructor function

                // interface count
                p = stack.Pop() as ActionPush;
                args = p.GetIntValue();

                // pop interfaces
                for (var i = 0; i < args; i++) stack.Pop();
                aimpl.NumInterfaces = args;
                break;

            // --------------------------------------

            case (int)ActionCode.DefineFunction:
                var f = a as ActionDefineFunction;
                trav = new CodeTraverser(f.ActionRecord);
                trav.Traverse(new InvocationExaminer());
                stack.Push(null);
                break;

            case (int)ActionCode.DefineFunction2:
                var f2 = a as ActionDefineFunction2;
                trav = new CodeTraverser(f2.ActionRecord);
                trav.Traverse(new InvocationExaminer());
                stack.Push(null);
                break;

            // --------------------------------------

            default:
                try
                {
                    for (var i = 0; i < a.PopCount; i++) stack.Pop();
                    for (var i = 0; i < a.PushCount; i++) stack.Push(null);
                }
                // stack empty
                catch (InvalidOperationException e)
                {
                    if (e != null)
                    {
                    }

                    stack.Clear();
                }

                break;
        }
    }
}