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
using System.Text;
using SWF.NET.ByteCode.Actions;
using SWF.NET.Utils;

namespace SWF.NET.ByteCode;

/// <summary>
///     Decompiler class. Compiles swf byte code to list
///     of <see cref="BaseAction">action objects</see>.
/// </summary>
public class Decompiler
{
    private int LabelId;

    // swf version info
    private int version;

    /// <summary>
    ///     constructor.
    /// </summary>
    /// <param name="version">swf Version</param>
    public Decompiler(int version) =>
        this.version = version;

    /// <summary>
    ///     Read <see cref="ActionIf">if</see> action from swf.
    /// </summary>
    private ActionIf ReadActionIf(BinaryReader br)
    {
        var len = Convert.ToInt32(br.ReadInt16());
        var o = br.ReadInt16();
        var a = new ActionIf(Convert.ToInt32(o));
        //a.ByteSize = len+3;
        return a;
    }

    /// <summary>
    ///     Read <see cref="ActionJump">jump</see> action from swf.
    /// </summary>
    private ActionJump ReadActionJump(BinaryReader br)
    {
        var len = Convert.ToInt32(br.ReadInt16());
        var o = br.ReadInt16();
        var a = new ActionJump(Convert.ToInt32(o));
        //a.ByteSize = len+3;			
        return a;
    }

    /// <summary>
    ///     Read <see cref="ActionConstantPool">constant pool</see> action from swf. the constant pool is not parsed.
    /// </summary>
    private ActionConstantPool ReadActionConstantPool(BinaryReader br)
    {
        // read block length
        var len = Convert.ToInt32(br.ReadUInt16());

        var constCount = Convert.ToInt32(br.ReadUInt16());
        var constantPool = new string[constCount];

        for (var i = 0; i < constCount; i++) constantPool[i] = BinaryStringRW.ReadString(br);
        var a = new ActionConstantPool(constantPool);
        //a.ByteSize = len+3;
        return a;
    }

    /// <summary>
    ///     Read <see cref="ActionStoreRegister">store register</see> action from swf.
    /// </summary>
    private ActionStoreRegister ReadActionStoreRegister(BinaryReader br)
    {
        var len = Convert.ToInt32(br.ReadUInt16());
        var a = new ActionStoreRegister(br.ReadByte());
        //a.ByteSize = len+3;
        return a;
    }

    /// <summary>
    ///     Read multiply push action action as <see cref="ActionPushList">ActionPushList</see> from swf.
    /// </summary>
    private ActionPushList ReadActionPush(BinaryReader br)
    {
        // read block length
        var len = Convert.ToInt32(br.ReadUInt16());
        var i = 0;
        var pushList = new ArrayList();

        while (i < len)
        {
            var pushType = Convert.ToInt32(br.ReadByte());
            i++;

            var val = new object();

            switch (pushType)
            {
                case 0:
                    var str = BinaryStringRW.ReadString(br);
                    i += str.Length + 1;
                    val = str;
                    break;

                case 1:
                    val = br.ReadSingle();
                    i += 4;
                    break;

                case 2:
                    val = null;
                    break;

                case 3:
                    val = null;
                    break;

                case 4:
                    val = Convert.ToInt32(br.ReadByte());
                    i++;
                    break;

                case 5:
                    val = br.ReadBoolean();
                    i++;
                    break;

                case 6:
                    var b0 = br.ReadBytes(4);
                    var b1 = br.ReadBytes(4);
                    var b = new byte[8];
                    b0.CopyTo(b, 4);
                    b1.CopyTo(b, 0);
                    val = BitConverter.ToDouble(b, 0);
                    i += 8;
                    break;

                case 7:
                    val = br.ReadInt32();
                    i += 4;
                    break;

                case 8:
                    val = Convert.ToInt32(br.ReadByte());
                    i++;
                    break;

                case 9:
                    val = Convert.ToInt32(br.ReadUInt16());
                    i += 2;
                    break;
            }

            var p = new ActionPush(pushType, val);
            pushList.Add(p);
        }

        var pList = new ActionPush[pushList.Count];
        pushList.CopyTo(pList, 0);
        var a = new ActionPushList(pList);

        //a.ByteSize = len+3;
        return a;
    }

    /// <summary>
    ///     Read <see cref="ActionPushList">ActionDefineFunction</see> from swf.
    ///     including inner actions
    /// </summary>
    private ActionDefineFunction ReadActionDefineFunction(BinaryReader br)
    {
        var start = Convert.ToInt32(br.BaseStream.Position);

        // read block length
        var len = Convert.ToInt32(br.ReadUInt16());

        var name = BinaryStringRW.ReadString(br);
        var numParams = Convert.ToInt32(br.ReadUInt16());
        var parameterList = new string[numParams];
        for (var i = 0; i < numParams; i++) parameterList[i] = BinaryStringRW.ReadString(br);

        var blockSize = Convert.ToInt32(br.ReadUInt16());

        // read function body
        var InnerActions = ReadActions(br.ReadBytes(blockSize));
        var a = new ActionDefineFunction(name, parameterList, InnerActions);

        var end = Convert.ToInt32(br.BaseStream.Position);
        //a.ByteSize = end-start +1;

        return a;
    }

    /// <summary>
    ///     Read <see cref="ActionPushList">ActionDefineFunction2</see> from swf.
    ///     including inner actions
    /// </summary>
    private ActionDefineFunction2 ReadActionDefineFunction2(BinaryReader br)
    {
        var start = Convert.ToInt32(br.BaseStream.Position);
        // read block length
        var len = Convert.ToInt32(br.ReadUInt16());

        var name = BinaryStringRW.ReadString(br);
        var numParams = Convert.ToInt32(br.ReadUInt16());
        var numRegs = Convert.ToInt32(br.ReadByte());
        var flags1 = br.ReadByte();
        var flags2 = br.ReadByte();

        var f
            = new ActionDefineFunction2.VariableFlagSet(
                (flags1 & 0x80) == 0x80,
                (flags1 & 0x40) == 0x40,
                (flags1 & 0x20) == 0x20,
                (flags1 & 0x10) == 0x10,
                (flags1 & 0x08) == 0x08,
                (flags1 & 0x04) == 0x04,
                (flags1 & 0x02) == 0x02,
                (flags1 & 0x01) == 0x01,
                (flags2 & 0x01) == 0x01
            );

        // read parameters
        var paramList = new ActionDefineFunction2.RegParamPair[numParams];

        for (var i = 0; i < numParams; i++)
        {
            var r = Convert.ToInt32(br.ReadByte());
            var p = BinaryStringRW.ReadString(br);
            paramList[i] = new ActionDefineFunction2.RegParamPair(r, p);
        }

        var blockSize = Convert.ToInt32(br.ReadUInt16());

        // read function body
        var InnerActions = ReadActions(br.ReadBytes(blockSize));
        var a = new ActionDefineFunction2(name, paramList, numRegs, f, InnerActions);

        var end = Convert.ToInt32(br.BaseStream.Position);
        ////a.ByteSize = len+3+blockSize;
        //a.ByteSize = end-start+1;		

        return a;
    }

    /// <summary>
    ///     Read <see cref="ActionSetTarget">ActionSetTarget</see> from swf.
    /// </summary>
    private ActionSetTarget ReadActionSetTarget(BinaryReader br)
    {
        var len = Convert.ToInt32(br.ReadUInt16());

        var a = new ActionSetTarget(BinaryStringRW.ReadString(br));
        //a.ByteSize = len+3; 

        return a;
    }

    /// <summary>
    ///     Read <see cref="ActionGotoFrame">ActionGotoFrame</see> from swf.
    /// </summary>
    private ActionGotoFrame ReadActionGotoFrame(BinaryReader br)
    {
        var len = Convert.ToInt32(br.ReadUInt16());
        var f = br.ReadInt16();

        var a = new ActionGotoFrame(f);
        //a.ByteSize = len+3;

        return a;
    }

    /// <summary>
    ///     Read <see cref="ActionGotoFrame2">ActionGotoFrame2</see> from swf.
    /// </summary>
    private ActionGotoFrame2 ReadActionGotoFrame2(BinaryReader br)
    {
        var len = Convert.ToInt32(br.ReadUInt16());

        var b = br.ReadBytes(len);

        var a = new ActionGotoFrame2(b);
        //a.ByteSize = len+3;

        return a;
    }

    /// <summary>
    ///     Read <see cref="ActionGotoLabel">ActionGotoLabel</see> from swf.
    /// </summary>
    private ActionGotoLabel ReadActionGotoLabel(BinaryReader br)
    {
        var len = Convert.ToInt32(br.ReadUInt16());
        var label = BinaryStringRW.ReadString(br);

        var a = new ActionGotoLabel(label);
        //a.ByteSize = len+3;

        return a;
    }


    /// <summary>
    ///     Read <see cref="ActionGetUrl">ActionGetUrl</see> from swf.
    /// </summary>
    private ActionGetUrl ReadActionGetUrl(BinaryReader br)
    {
        var len = Convert.ToInt32(br.ReadUInt16());

        var urlStr = BinaryStringRW.ReadString(br);
        var targetStr = BinaryStringRW.ReadString(br);

        var a = new ActionGetUrl(urlStr, targetStr);
        //a.ByteSize = len+3; 			

        return a;
    }

    /// <summary>
    ///     Read <see cref="ActionGetUrl2">ActionGetUrl2</see> from swf.
    /// </summary>
    private ActionGetUrl2 ReadActionGetUrl2(BinaryReader br)
    {
        var len = Convert.ToInt32(br.ReadUInt16());
        var a = new ActionGetUrl2(br.ReadByte());
        //a.ByteSize = len+3; 			

        return a;
    }


    /// <summary>
    ///     Read <see cref="ActionWaitForFrame">ActionWaitForFrame</see> from swf.
    /// </summary>
    private ActionWaitForFrame ReadActionWaitForFrame(BinaryReader br)
    {
        var len = Convert.ToInt32(br.ReadUInt16());
        var frame = br.ReadInt16();
        var skip = br.ReadByte();

        var a = new ActionWaitForFrame(frame, skip);
        //a.ByteSize = len+3; 			

        return a;
    }

    /// <summary>
    ///     Read <see cref="ActionWaitForFrame2">ActionWaitForFrame2</see> from swf.
    /// </summary>
    private ActionWaitForFrame2 ReadActionWaitForFrame2(BinaryReader br)
    {
        var len = Convert.ToInt32(br.ReadUInt16());
        var skip = br.ReadByte();

        var a = new ActionWaitForFrame2(skip);
        //a.ByteSize = len+3; 			

        return a;
    }


    /// <summary>
    ///     Read <see cref="ActionWith">ActionWith</see> from swf.
    /// </summary>
    private ActionWith ReadActionWith(BinaryReader br)
    {
        var len = Convert.ToInt32(br.ReadUInt16());
        var block = br.ReadUInt16();

        var a = new ActionWith(block);
        //a.ByteSize = len+3; 			

        return a;
    }


    /// <summary>
    ///     Read try/catch block from swf and create corresponding
    ///     <see cref="ActionTry">ActionTry</see>,
    ///     <see cref="ActionCatch">ActionCatch</see>,
    ///     <see cref="ActionFinally">ActionFinally</see>,
    ///     <see cref="ActionEndTryBlock">ActionEndTryBlock</see>
    ///     actions.
    /// </summary>
    private ActionTry ReadActionTry(BinaryReader br)
    {
        br.ReadUInt16();

        var startStream = br.BaseStream.Position;

        var flags = br.ReadByte();

        var catchInRegister = (flags & 0x04) == 0x04;
        var finallyFlag = (flags & 0x02) == 0x02;
        var catchFlag = (flags & 0x01) == 0x01;

        var trySize = br.ReadUInt16();
        var catchSize = br.ReadUInt16();
        var finallySize = br.ReadUInt16();

        var catchName = "";
        byte catchRegister = 0;

        if (catchInRegister)
            catchRegister = br.ReadByte();
        else
            catchName = BinaryStringRW.ReadString(br);

        var len = Convert.ToInt32(br.BaseStream.Position - startStream);

        var a = new ActionTry(
            catchInRegister,
            finallyFlag,
            catchFlag,
            trySize,
            catchSize,
            finallySize,
            catchName,
            catchRegister);

        //a.ByteSize = len+3;

        return a;
    }


    /// <summary>
    ///     Read unknown instruction as <see cref="UnknownAction">UnknownAction</see> object
    /// </summary>
    private UnknownAction ReadUnknownAction(byte code, BinaryReader br)
    {
        byte[] bytecode;

        if (code < 0x80)
            bytecode = new byte[1] { code };
        else
        {
            var len = Convert.ToInt32(br.ReadUInt16());
            br.BaseStream.Position -= 3;
            bytecode = br.ReadBytes(len + 3);
        }

        var u = new UnknownAction(code, bytecode);
        //u.ByteSize = bytecode.Length;

        return u;
    }

    /// <summary>
    ///     Read actions according to action code in swf
    /// </summary>
    private BaseAction ReadAction(BinaryReader br)
    {
        var bytecode = br.ReadByte();

        switch ((ActionCode)Convert.ToInt32(bytecode))
        {
            // singlebyte actions
            case ActionCode.End: return new ActionEnd();
            case ActionCode.NextFrame: return new ActionNextFrame();
            case ActionCode.PreviousFrame: return new ActionPreviousFrame();
            case ActionCode.Play: return new ActionPlay();
            case ActionCode.Stop: return new ActionStop();
            case ActionCode.ToggleQuality: return new ActionToggleQuality();
            case ActionCode.StopSounds: return new ActionStopSounds();
            case ActionCode.Pop: return new ActionPop();
            case ActionCode.Add: return new ActionAdd();
            case ActionCode.Subtract: return new ActionSubtract();
            case ActionCode.Multiply: return new ActionMultiply();
            case ActionCode.Divide: return new ActionDivide();
            case ActionCode.Equals: return new ActionEquals();
            case ActionCode.Less: return new ActionLess();
            case ActionCode.And: return new ActionAnd();
            case ActionCode.Or: return new ActionOr();
            case ActionCode.Not: return new ActionNot();
            case ActionCode.StringAdd: return new ActionStringAdd();
            case ActionCode.StringEquals: return new ActionStringEquals();
            case ActionCode.StringExtract: return new ActionStringExtract();
            case ActionCode.StringLength: return new ActionStringLength();
            case ActionCode.StringLess: return new ActionStringLess();
            case ActionCode.MBStringExtract: return new ActionMBStringExtract();
            case ActionCode.MBStringLength: return new ActionMBStringLength();
            case ActionCode.AsciiToChar: return new ActionAsciiToChar();
            case ActionCode.CharToAscii: return new ActionCharToAscii();
            case ActionCode.ToInteger: return new ActionToInteger();
            case ActionCode.MBAsciiToChar: return new ActionMBAsciiToChar();
            case ActionCode.MBCharToAscii: return new ActionMBCharToAscii();
            case ActionCode.Call: return new ActionCall();
            case ActionCode.GetVariable: return new ActionGetVariable();
            case ActionCode.SetVariable: return new ActionSetVariable();
            case ActionCode.GetProperty: return new ActionGetProperty();
            case ActionCode.RemoveSprite: return new ActionRemoveSprite();
            case ActionCode.SetProperty: return new ActionSetProperty();
            case ActionCode.SetTarget2: return new ActionSetTarget2();
            case ActionCode.StartDrag: return new ActionStartDrag();
            case ActionCode.CloneSprite: return new ActionCloneSprite();
            case ActionCode.EndDrag: return new ActionEndDrag();
            case ActionCode.GetTime: return new ActionGetTime();
            case ActionCode.RandomNumber: return new ActionRandomNumber();
            case ActionCode.Trace: return new ActionTrace();
            case ActionCode.CallFunction: return new ActionCallFunction();
            case ActionCode.CallMethod: return new ActionCallMethod();
            case ActionCode.DefineLocal: return new ActionDefineLocal();
            case ActionCode.DefineLocal2: return new ActionDefineLocal2();
            case ActionCode.Delete: return new ActionDelete();
            case ActionCode.Delete2: return new ActionDelete2();
            case ActionCode.Enumerate: return new ActionEnumerate();
            case ActionCode.Equals2: return new ActionEquals2();
            case ActionCode.GetMember: return new ActionGetMember();
            case ActionCode.InitArray: return new ActionInitArray();
            case ActionCode.InitObject: return new ActionInitObject();
            case ActionCode.NewMethod: return new ActionNewMethod();
            case ActionCode.NewObject: return new ActionNewObject();
            case ActionCode.SetMember: return new ActionSetMember();
            case ActionCode.TargetPath: return new ActionTargetPath();
            case ActionCode.ToNumber: return new ActionToNumber();
            case ActionCode.ToString: return new ActionToString();
            case ActionCode.TypeOf: return new ActionTypeOf();
            case ActionCode.Add2: return new ActionAdd2();
            case ActionCode.Less2: return new ActionLess2();
            case ActionCode.Modulo: return new ActionModulo();
            case ActionCode.BitAnd: return new ActionBitAnd();
            case ActionCode.BitLShift: return new ActionBitLShift();
            case ActionCode.BitOr: return new ActionBitOr();
            case ActionCode.BitRShift: return new ActionBitRShift();
            case ActionCode.BitURShift: return new ActionBitURShift();
            case ActionCode.BitXor: return new ActionBitXor();
            case ActionCode.Decrement: return new ActionDecrement();
            case ActionCode.Increment: return new ActionIncrement();
            case ActionCode.PushDuplicate: return new ActionPushDuplicate();
            case ActionCode.Return: return new ActionReturn();
            case ActionCode.StackSwap: return new ActionStackSwap();
            case ActionCode.InstanceOf: return new ActionInstanceOf();
            case ActionCode.Enumerate2: return new ActionEnumerate2();
            case ActionCode.StrictEquals: return new ActionStrictEquals();
            case ActionCode.Greater: return new ActionGreater();
            case ActionCode.StringGreater: return new ActionStringGreater();
            case ActionCode.Extends: return new ActionExtends();
            case ActionCode.CastOp: return new ActionCastOp();
            case ActionCode.Implements: return new ActionImplements();
            case ActionCode.Throw: return new ActionThrow();

            // multibyte actions
            case ActionCode.ConstantPool: return ReadActionConstantPool(br);
            case ActionCode.GetURL: return ReadActionGetUrl(br);
            case ActionCode.GetURL2: return ReadActionGetUrl2(br);
            case ActionCode.WaitForFrame: return ReadActionWaitForFrame(br);
            case ActionCode.WaitForFrame2: return ReadActionWaitForFrame2(br);
            case ActionCode.GotoFrame: return ReadActionGotoFrame(br);
            case ActionCode.GotoFrame2: return ReadActionGotoFrame2(br);
            case ActionCode.GoToLabel: return ReadActionGotoLabel(br);
            case ActionCode.SetTarget: return ReadActionSetTarget(br);
            case ActionCode.With: return ReadActionWith(br);
            case ActionCode.Try: return ReadActionTry(br);
            case ActionCode.Push: return ReadActionPush(br);
            case ActionCode.StoreRegister: return ReadActionStoreRegister(br);
            case ActionCode.Jump: return ReadActionJump(br);
            case ActionCode.If: return ReadActionIf(br);
            case ActionCode.DefineFunction: return ReadActionDefineFunction(br);
            case ActionCode.DefineFunction2: return ReadActionDefineFunction2(br);
        }

        return ReadUnknownAction(bytecode, br);
    }

    /// <summary>
    ///     Read bytecode actions from swf
    /// </summary>
    private ArrayList ReadActions(byte[] codeblock)
    {
        var actionsRead = new ArrayList();

        // create binary reader
        var stream = new MemoryStream(codeblock, false);
        var reader = new BinaryReader(stream, Encoding.UTF8);

        // read bytecode sequenz
        while (reader.PeekChar() != -1)
        {
            // read
            var a = ReadAction(reader);

            // define constant pool
            actionsRead.Add(a);
        }

        CreateBranchLabels(actionsRead);
        CreatePseudoActions(actionsRead);

        return actionsRead;
    }

    /// <summary>
    ///     convert <see cref="ActionPushList">push list</see> to sequence of single <see cref="ActionPush">push</see> actions
    /// </summary>
    private void ExplodePushLists(ArrayList actionRecord)
    {
        for (var i = 0; i < actionRecord.Count; i++)
        {
            var a = (BaseAction)actionRecord[i];

            // check if action is multiple push
            var pl = actionRecord[i] as ActionPushList;
            if (pl != null)
            {
                // resolve pushs to single push actions
                for (var j = 0; j < pl.Length; j++)
                {
                    var p = pl[j];
                    actionRecord.Insert(i + 1 + j, p);
                }

                actionRecord.RemoveAt(i);
            }

            // process inner actionblocks 				
            if (a as ActionDefineFunction != null)
            {
                var f = (ActionDefineFunction)a;
                ExplodePushLists(f.ActionRecord);
            }

            if (a as ActionDefineFunction2 != null)
            {
                var f = (ActionDefineFunction2)a;
                ExplodePushLists(f.ActionRecord);
            }
        }
    }

    /// <summary>
    ///     create <see cref="ActionLabel">ActionLabel</see> pseudo actions for branch labels
    /// </summary>
    private void CreateBranchLabels(ArrayList actionRecord)
    {
        var labelList = new SortedList();

        var idx = 0;
        while (idx < actionRecord.Count)
        {
            // read action
            var a = (BaseAction)actionRecord[idx];

            // check if action is branch
            if (a as IJump != null)
            {
                var jump = (IJump)a;
                var offset = jump.Offset;
                var sidx = idx;

                if (offset < 0)
                {
                    // back branch
                    offset += a.ByteCount;

                    while (offset < 0)
                    {
                        sidx--;
                        if (sidx < 0) break;

                        var ac = (BaseAction)actionRecord[sidx];
                        offset += ac.ByteCount;
                    }

                    if (!labelList.ContainsKey(sidx))
                    {
                        LabelId++;
                        labelList[sidx] = LabelId;
                        jump.LabelId = LabelId;
                    }
                    else
                        jump.LabelId = (int)labelList[sidx];
                }
                else
                {
                    if (offset == 0)
                    {
                        sidx = idx + 1;
                        if (!labelList.ContainsKey(sidx))
                        {
                            LabelId++;
                            labelList[sidx] = LabelId;
                            jump.LabelId = LabelId;
                        }
                        else
                            jump.LabelId = (int)labelList[sidx];
                    }
                    else
                    {
                        // offset>0
                        do
                        {
                            sidx++;
                            if (sidx >= actionRecord.Count) break;

                            var ac = (BaseAction)actionRecord[sidx];
                            offset -= ac.ByteCount;
                        }
                        while (offset > 0);

                        sidx++;
                        if (!labelList.ContainsKey(sidx))
                        {
                            LabelId++;
                            labelList[sidx] = LabelId;
                            jump.LabelId = LabelId;
                        }
                        else
                            jump.LabelId = (int)labelList[sidx];
                    }
                }
            }

            idx++;
        }

        var lines = new ArrayList(labelList.GetKeyList());

        foreach (int line in lines)
        {
            var label = (int)labelList[line];
            if (line < actionRecord.Count)
            {
                var a = (BaseAction)actionRecord[line];
                actionRecord[line] = new ActionContainer(
                    new BaseAction[2] { new ActionLabel(label), a }
                );
            }
            else
                actionRecord.Add(new ActionLabel(label));
        }

        idx = 0;
        while (idx < actionRecord.Count)
        {
            var a = (BaseAction)actionRecord[idx];
            if (a is ActionContainer)
            {
                var bl = ((ActionContainer)a).ActionList;
                actionRecord.RemoveAt(idx);
                var j = 0;
                while (j < bl.Length)
                {
                    actionRecord.Insert(idx, bl[j]);
                    j++;
                    idx++;
                }

                continue;
            }

            idx++;
        }
    }

    /// <summary>
    ///     create other pseudo actions
    /// </summary>
    private void CreatePseudoActions(ArrayList actionRecord)
    {
        for (var i = 0; i < actionRecord.Count; i++)
        {
            var a = (BaseAction)actionRecord[i];

            // -----------------------
            // try/catch/finally block
            // -----------------------

            var aTry = a as ActionTry;
            if (aTry != null)
            {
                var j = i + 1;
                var offset = aTry.SizeTry;
                // skip try block
                while (offset > 0)
                {
                    var currentAction = (BaseAction)actionRecord[j];
                    offset -= currentAction.ByteCount;
                    j++;
                }

                // skip catch
                if (aTry.SizeCatch > 0)
                {
                    actionRecord.Insert(j, new ActionCatch());
                    j++;
                    offset = aTry.SizeCatch;
                    while (offset > 0)
                    {
                        var currentAction = (BaseAction)actionRecord[j];
                        offset -= currentAction.ByteCount;
                        j++;
                    }
                }

                // skip finally
                if (aTry.SizeFinally > 0)
                {
                    actionRecord.Insert(j, new ActionFinally());
                    j++;
                    offset = aTry.SizeFinally;
                    while (offset > 0)
                    {
                        var currentAction = (BaseAction)actionRecord[j];
                        offset -= currentAction.ByteCount;
                        j++;
                    }
                }

                // end
                actionRecord.Insert(j, new ActionEndTryBlock());
            }

            // -----------------------
            // 			with
            // -----------------------

            var aWith = a as ActionWith;

            if (aWith != null)
            {
                var j = i + 1;
                var offset = aWith.BlockLength;
                while (offset > 0)
                {
                    var currentAction = (BaseAction)actionRecord[j];
                    offset -= currentAction.ByteCount;
                    j++;
                }

                actionRecord.Insert(j, new ActionEndWith());
            }

            // -----------------------
            // 	  wait for frame
            // -----------------------

            var aWait = a as ActionWaitForFrame;
            if (aWait != null)
            {
                var j = i + 1;
                var count = aWait.SkipCount;
                while (count > 0)
                {
                    if (((BaseAction)actionRecord[j]).Code >= 0x00 || ((BaseAction)actionRecord[j]).Code == (int)ActionCode.PushList)
                        count--;
                    j++;
                }

                actionRecord.Insert(j, new ActionEndWait());
            }

            var aWait2 = a as ActionWaitForFrame2;
            if (aWait2 != null)
            {
                var j = i + 1;
                var count = aWait2.SkipCount;
                while (count > 0)
                {
                    if (((BaseAction)actionRecord[j]).Code >= 0x00 || ((BaseAction)actionRecord[j]).Code == (int)ActionCode.PushList)
                        count--;
                    j++;
                }

                actionRecord.Insert(j, new ActionEndWait());
            }
        }
    }

    /// <summary>
    ///     decompile byte code to action objects
    /// </summary>
    public ArrayList Decompile(byte[] codeblock)
    {
        LabelId = 1;
        var actionRec = ReadActions(codeblock);
        ExplodePushLists(actionRec);

        // find argument count on stack 
        // e.g. for actions likecallFunction or initArray		
        var trav = new CodeTraverser(actionRec);
        trav.Traverse(new InvocationExaminer());

        return actionRec;
    }
}