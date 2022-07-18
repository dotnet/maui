using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Native
{
	/// <summary>
	/// Event arguments for <see cref="DatePicker.DateSelected"/> event.
	/// </summary>
	public class DateChangedEventArgs : EventArgs
	{
		/// <summary>
		/// The date that the user entered.
		/// </summary>
		public DateTime NewDate { get; }

		/// <summary>
		/// Creates a new <see cref="DateChangedEventArgs"/> object that represents a change to <paramref name="newDate"/>.
		/// </summary>
		/// <param name="newDate">Current date of <see cref="DatePicker"/>.</param>
		public DateChangedEventArgs(DateTime newDate)
		{
			NewDate = newDate;
		}
	}
}
