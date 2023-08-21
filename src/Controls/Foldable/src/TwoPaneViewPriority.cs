// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Controls.Foldable
{
	/// <summary>
	/// Defines constants that specify which pane has priority in a TwoPaneView.
	/// Affects the rendering of <see cref="Microsoft.Maui.Controls.Foldable.TwoPaneViewTallModeConfiguration"/> and 
	/// <see cref="Microsoft.Maui.Controls.Foldable.TwoPaneViewWideModeConfiguration"/>
	/// </summary>
	public enum TwoPaneViewPriority
	{
		/// <summary>
		/// Pane 1 has priority.
		/// </summary>
		Pane1,
		/// <summary>
		/// Pane 2 has priority.
		/// </summary>
		Pane2
	}
}
