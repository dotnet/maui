#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event Args for <see cref="Microsoft.Maui.Controls.CheckBox"/>'s <see cref="Microsoft.Maui.Controls.CheckBox.CheckedChanged"/> event.</summary>
	public class CheckedChangedEventArgs : EventArgs
	{
		/// <summary>Creates a new <see cref="Microsoft.Maui.Controls.CheckedChangedEventArgs"/> specifying whether the <see cref="Microsoft.Maui.Controls.CheckBox"/> is checked.</summary>
		/// <param name="value">Boolean value indicating whether the <see cref="Microsoft.Maui.Controls.CheckBox"/> is checked.</param>
		public CheckedChangedEventArgs(bool value)
		{
			Value = value;
			CheckedValue = value;
		}

		internal CheckedChangedEventArgs(bool? value)
		{
			CheckedValue = value;
			Value = value == true;
		}

		/// <summary>Gets true only when checked; false when unchecked or indeterminate.</summary>
		public bool Value { get; private set; }

		/// <summary>Gets the complete checked state, or null when indeterminate.</summary>
		public bool? CheckedValue { get; }
	}
}