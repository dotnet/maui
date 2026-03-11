#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for numeric value changes.</summary>
	public class ValueChangedEventArgs : EventArgs
	{
		/// <summary>Creates a new <see cref="ValueChangedEventArgs"/> with the old and new values.</summary>
		/// <param name="oldValue">The previous value.</param>
		/// <param name="newValue">The new value.</param>
		public ValueChangedEventArgs(double oldValue, double newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}

		/// <summary>Gets the new value.</summary>
		public double NewValue { get; private set; }

		/// <summary>Gets the previous value.</summary>
		public double OldValue { get; private set; }
	}
}