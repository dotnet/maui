// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a top-level menu in a MenuBar view.
	/// </summary>
	public interface IMenuBarItem : IList<IMenuElement>, IElement
	{
		string Text { get; }

		/// <summary>
		/// Gets a value indicating whether this View is enabled in the user interface. 
		/// </summary>
		bool IsEnabled { get; }
	}
}
