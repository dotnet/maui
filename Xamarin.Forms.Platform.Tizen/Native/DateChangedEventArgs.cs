using System;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	/// <summary>
	/// Event arguments for <see cref="DatePicker.DateChanged"/> event.
	/// </summary>
	public class DateChangedEventArgs : EventArgs
	{
		/// <summary>
		/// The date that the user entered.
		/// </summary>
		public DateTime NewDate { get; }

		/// <summary>
		/// Creates a new <see cref="DateChangedEventArgs"/> object that represents a change from <paramref name="oldDate"/> to <paramref name="newDate"/>.
		/// </summary>
		/// <param name="oldDate">Old date of <see cref="DatePicker"/>.</param>
		/// <param name="newDate">Current date of <see cref="DatePicker"/>.</param>
		public DateChangedEventArgs(DateTime newDate)
		{
			NewDate = newDate;
		}
	}
}
