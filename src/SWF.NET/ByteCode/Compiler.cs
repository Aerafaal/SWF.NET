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

using System.Collections;
using System.IO;
using System.Text;
using SWF.NET.ByteCode.Actions;

namespace SWF.NET.ByteCode;

/// <summary>
///     Compiler class, exposes one public method: <see cref="Compile" />.
/// </summary>
public class Compiler
{
    // writer
    private readonly BinaryWriter binWriter;
    private readonly MemoryStream memStream;

    /// <summary>
    ///     Constructor.
    /// </summary>
    public Compiler()
    {
        memStream = new MemoryStream();
        binWriter = new BinaryWriter(memStream, Encoding.Default);
    }

    /// <summary>
    ///     Collaps sequence of single push actions into one multiple-push action
    /// </summary>
    private void CollapsPushActions(ArrayList actionRecord)
    {
        var i = 0;
        bool isPush;

        while (i < actionRecord.Count - 1)
        {
            isPush = actionRecord[i] is ActionPush;
            if (isPush)
            {
                var j = i;
                var count = 1;

                do
                {
                    i++;
                    if (i < actionRecord.Count)
                    {
                        isPush = actionRecord[i] is ActionPush;
                        if (isPush) count++;
                    }
                }
                while (isPush && i < actionRecord.Count);

                if (count > 1)
                {
                    var pushList = new ActionPush[count];
                    actionRecord.CopyTo(j, pushList, 0, count);

                    actionRecord.RemoveRange(j, count);
                    var pl = new ActionPushList(pushList);
                    actionRecord.Insert(j, pl);

                    i = j + 1;
                }
            }
            else
            {
                // recursively step through functions inner actions
                var f = actionRecord[i] as ActionDefineFunction;
                if (f != null) CollapsPushActions(f.ActionRecord);

                // and function2 of course
                var f2 = actionRecord[i] as ActionDefineFunction2;
                if (f2 != null) CollapsPushActions(f2.ActionRecord);
                i++;
            }
        }
    }

    /// <summary>
    ///     Calculate branch offsets.
    /// </summary>
    private void CalcBranchOffsets(ArrayList actionRecord)
    {
        if (actionRecord.Count < 1) return;

        var jumpList = new ArrayList();
        var labelPos = new Hashtable();

        var pos = 0;
        for (var i = 0; i < actionRecord.Count; i++)
        {
            var action = (BaseAction)actionRecord[i];
            var label = action as ActionLabel;
            var jump = action as IJump;

            if (label != null) labelPos[label.LabelId] = pos;

            if (jump != null) jumpList.Add(new JumpPos(pos, jump));

            // recursively step through function blocks
            var f = actionRecord[i] as ActionDefineFunction;
            if (f != null) CalcBranchOffsets(f.ActionRecord);

            var f2 = actionRecord[i] as ActionDefineFunction2;
            if (f2 != null) CalcBranchOffsets(f2.ActionRecord);

            pos += action.ByteCount;
        }

        for (var i = 0; i < jumpList.Count; i++)
        {
            var j = (JumpPos)jumpList[i];
            var offset = (int)labelPos[j.Jump.LabelId] - j.Position - 5;
            j.Jump.Offset = offset;
        }
    }

    /// <summary>
    ///     Calculate size or offset for action blocks.
    /// </summary>
    private void CalcBlockOffsets(ArrayList actionRecord)
    {
        if (actionRecord.Count < 1) return;

        for (var i = 0; i < actionRecord.Count; i++)
        {
            var a = (BaseAction)actionRecord[i];

            // action with
            var aWith = a as ActionWith;
            if (aWith != null)
            {
                var j = i;
                var offset = 0;
                do
                {
                    j++;
                    offset += ((BaseAction)actionRecord[j]).ByteCount;
                }
                while (actionRecord[j] as ActionEndWith == null);

                var oldOffset = aWith.BlockLength;

                aWith.BlockLength = offset;
            }

            // action waitForFrame
            var aWait = a as ActionWaitForFrame;
            if (aWait != null)
            {
                var j = i;
                var count = 0;
                BaseAction ca;
                do
                {
                    j++;
                    ca = (BaseAction)actionRecord[j];
                    if (ca.Code >= 0 || ca.Code == (int)ActionCode.PushList) count++;
                }
                while (ca as ActionEndWait == null);

                aWait.SkipCount = count;
            }

            // action waitForFrame2
            var aWait2 = a as ActionWaitForFrame2;
            if (aWait2 != null)
            {
                var j = i;
                var count = 0;
                BaseAction ca;
                do
                {
                    j++;
                    ca = (BaseAction)actionRecord[j];
                    if (ca.Code >= 0 || ca.Code == (int)ActionCode.PushList) count++;
                }
                while (ca as ActionEndWait == null);

                aWait2.SkipCount = count;
            }

            // action function
            var f = actionRecord[i] as ActionDefineFunction;
            if (f != null) CalcBlockOffsets(f.ActionRecord);

            // action function2
            var f2 = actionRecord[i] as ActionDefineFunction2;
            if (f2 != null) CalcBlockOffsets(f2.ActionRecord);
        }
    }

    /// <summary>
    ///     Compile list of Action objects to byte code.
    /// </summary>
    /// <param name="actionRecord">List of <see cref="BaseAction">action objects</see></param>
    public byte[] Compile(ArrayList actionRecord)
    {
        // code blocks
        CollapsPushActions(actionRecord);
        CalcBranchOffsets(actionRecord);
        CalcBlockOffsets(actionRecord);

        // compile action-by-action
        foreach (var o in actionRecord)
        {
            var action = (BaseAction)o;
            action.Compile(binWriter);
        }

        return memStream.ToArray();
    }

    /// <summary>
    ///     Inner struct for storing branch data.
    /// </summary>
    private struct JumpPos
    {
        public readonly int Position;
        public readonly IJump Jump;

        public JumpPos(int pos, IJump j)
        {
            Position = pos;
            Jump = j;
        }
    }
}