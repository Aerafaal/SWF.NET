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

using System.Xml;

namespace SWF.NET;

/// <summary>
///     Component.
/// </summary>
public class Component
{
    #region Ctor

    /// <summary>
    ///     Creates a new <see cref="Component" /> instance.
    /// </summary>
    public Component()
    {
    }

    #endregion

    #region Members

    //private string actionScriptFileName;

    #endregion

    #region Properties

    /// <summary>
    ///     Gets or sets the SWF.
    /// </summary>
    /// <value></value>
    public Swf Swf { get; set; }

    /// <summary>
    ///     Gets or sets the name of the SWF file.
    /// </summary>
    /// <value></value>
    public string SwfFileName { get; set; }

    /// <summary>
    ///     Gets or sets the catalog.
    /// </summary>
    /// <value></value>
    public XmlDocument Catalog { get; set; }

    /// <summary>
    ///     Gets or sets the name of the catalog file.
    /// </summary>
    /// <value></value>
    public string CatalogFileName { get; set; }

    #endregion
}