/*
	SwfDotNet is an open source library for writing and reading 
	Macromedia Flash (SWF) bytecode.
	Copyright (C) 2005 Olivier Carpentier - Adelina foundation
	see Licence.cs for GPL full text!
		
	SwfDotNet.IO uses a part of the open source library SwfOp actionscript 
	byte code management, writted by Florian Kr�sch, Copyright (C) 2004 .
	
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

namespace SWF.NET.Tags;

#region Delegates

/// <summary>
///     Tag added in the collection EventHandler
/// </summary>
public delegate void TagAddedEvent(BaseTag tagAdded);

#endregion

/// <summary>
///     Base Tag collection
/// </summary>
public class BaseTagCollection : CollectionBase
{
    #region Ctor

    /// <summary>
    ///     Constructor
    /// </summary>
    public BaseTagCollection()
    {
    }

    #endregion

    #region Event

    /// <summary>
    ///     Tag added event
    /// </summary>
    public event TagAddedEvent TagAdded;

    #endregion

    #region Collection Methods

    /// <summary>
    ///     Clear collection
    /// </summary>
    public new void Clear() =>
        List.Clear();

    /// <summary>
    ///     Add a base tag
    /// </summary>
    /// <param name="value">base tag to add</param>
    /// <returns>base tag added</returns>
    public BaseTag Add(BaseTag value)
    {
        List.Add(value);

        try
        {
            TagAdded(value);
        }
        catch (Exception)
        {
        }

        return value;
    }

    /// <summary>
    ///     Adds the range.
    /// </summary>
    /// <param name="values">The values.</param>
    public void AddRange(BaseTagCollection values)
    {
        var enums = values.GetEnumerator();
        while (enums.MoveNext())
            Add((BaseTag)enums.Current);
    }


    /// <summary>
    ///     Add an array of base tag
    /// </summary>
    /// <param name="values">base tag array</param>
    public void AddRange(BaseTag[] values)
    {
        var enums = values.GetEnumerator();
        while (enums.MoveNext())
            Add((BaseTag)enums.Current);
    }

    /// <summary>
    ///     Remove a base tag
    /// </summary>
    /// <param name="value"></param>
    public void Remove(BaseTag value)
    {
        if (List.Contains(value))
            List.Remove(value);
    }

    /// <summary>
    ///     Insert a base tag at
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="value">base tag</param>
    public void Insert(int index, BaseTag value)
    {
        List.Insert(index, value);

        try
        {
            TagAdded(value);
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    ///     Test if list contain a base tag
    /// </summary>
    /// <param name="value">base tag</param>
    /// <returns>contain result</returns>
    public bool Contains(BaseTag value) =>
        List.Contains(value);

    /// <summary>
    ///     Access base tag list by index
    /// </summary>
    public BaseTag this[int index]
    {
        get => (BaseTag)List[index];
        set => List[index] = value;
    }

    /// <summary>
    ///     Get index of a base tag
    /// </summary>
    /// <param name="value">base tag</param>
    /// <returns>base tag index if is contain, -1 else.</returns>
    public int IndexOf(BaseTag value) =>
        List.IndexOf(value);

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the last BaseTag of the collection.
    /// </summary>
    /// <returns></returns>
    public BaseTag GetLastOne()
    {
        if (Count == 0)
            return null;

        return this[Count - 1];
    }

    /// <summary>
    ///     Convert collection to array
    /// </summary>
    /// <returns>Base tags array</returns>
    public BaseTag[] ToArray()
    {
        BaseTag[] res = null;

        if (Count > 0)
        {
            res = new BaseTag[Count];
            for (var i = 0; i < Count; i++)
                res[i] = this[i];
        }

        return res;
    }

    #endregion
}