using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Defines flags that represent different interactive states of a view.
	/// </summary>
	[Flags]
	public enum ViewState
	{
		/// <summary>
		/// The default state of the view.
		/// </summary>
		Default = 0,

		/// <summary>
		/// The view is being hovered over (prelight state).
		/// </summary>
		Prelight = 1,

		/// <summary>
		/// The view is being pressed.
		/// </summary>
		Pressed = 1 << 1
	}
}