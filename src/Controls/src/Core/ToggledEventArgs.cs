using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for toggle state changes.</summary>
	public class ToggledEventArgs : EventArgs
	{
		/// <summary>Creates a new <see cref="ToggledEventArgs"/> with the specified value.</summary>
		/// <param name="value">The new toggle state.</param>
		public ToggledEventArgs(bool value)
		{
			Value = value;
		}

		/// <summary>Gets the new toggle state.</summary>
		public bool Value { get; private set; }
	}
}