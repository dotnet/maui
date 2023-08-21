// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a shortcut key for a <see cref="T:Microsoft.Maui.Controls.MenuItem" />.
	/// </summary>
	public interface IAccelerator
	{
		/// <summary>
		/// Specifies the virtual key used to modify another keypress. 
		/// </summary>
		public IReadOnlyList<string> Modifiers { get; }

		/// <summary>
		/// Specifies the values for the virtual key.
		/// </summary>
		public string Key { get; }
	}
}
