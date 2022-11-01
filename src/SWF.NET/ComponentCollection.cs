/*
	SwfDotNet.Components is an open source library for reading 
	Flash components (SWC files).
	Copyright (C) 2005 Olivier Carpentier - Adelina foundation
	see Licence.cs for GPL full text!
		
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

namespace SWF.NET;

/// <summary>
///     ComponentCollection
/// </summary>
public class ComponentCollection : CollectionBase
{
    #region Ctor

    /// <summary>
    ///     Constructor
    /// </summary>
    public ComponentCollection()
    {
    }

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
    public Component Add(Component value)
    {
        List.Add(value);
        return value;
    }

    /// <summary>
    ///     Add an array of component
    /// </summary>
    /// <param name="values">swf array</param>
    public void AddRange(Component[] values)
    {
        foreach (var ip in values)
            Add(ip);
    }

    /// <summary>
    ///     Remove a component
    /// </summary>
    /// <param name="value"></param>
    public void Remove(Component value)
    {
        if (List.Contains(value))
            List.Remove(value);
    }

    /// <summary>
    ///     Insert a component at
    /// </summary>
    /// <param name="index">index</param>
    /// <param name="value">swf</param>
    public void Insert(int index, Component value) =>
        List.Insert(index, value);

    /// <summary>
    ///     Test if list contain a component
    /// </summary>
    /// <param name="value">component</param>
    /// <returns>contain result</returns>
    public bool Contains(Component value) =>
        List.Contains(value);

    /// <summary>
    ///     Access component list by index
    /// </summary>
    public Component this[int index]
    {
        get => (Component)List[index];
        set => List[index] = value;
    }

    /// <summary>
    ///     Get index of a component
    /// </summary>
    /// <param name="value">component</param>
    /// <returns>swf index if is contain, -1 else.</returns>
    public int IndexOf(Component value) =>
        List.IndexOf(value);

    #endregion

    #region Methods

    /// <summary>
    ///     Gets the last component of the collection.
    /// </summary>
    /// <returns></returns>
    public Component GetLastOne()
    {
        if (Count == 0)
            return null;

        return this[Count - 1];
    }

    /// <summary>
    ///     Convert collection to array
    /// </summary>
    /// <returns>component array</returns>
    public Component[] ToArray()
    {
        Component[] res = null;

        if (Count > 0)
        {
            res = new Component[Count];
            for (var i = 0; i < Count; i++)
                res[i] = this[i];
        }

        return res;
    }

    #endregion
}