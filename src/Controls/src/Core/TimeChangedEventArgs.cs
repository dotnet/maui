#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for the <see cref="TimePicker.TimeSelected"/> event.</summary>
	public class TimeChangedEventArgs : EventArgs
	{
		/// <summary>Creates a new <see cref="TimeChangedEventArgs"/> with the specified old and new times.</summary>
		/// <param name="oldTime">The previously selected time.</param>
		/// <param name="newTime">The newly selected time.</param>
		public TimeChangedEventArgs(TimeSpan? oldTime, TimeSpan? newTime)
		{
			OldTime = oldTime;
			NewTime = newTime;
		}

		/// <summary>Gets the newly selected time.</summary>
		public TimeSpan? NewTime { get; private set; }

		/// <summary>Gets the previously selected time.</summary>
		public TimeSpan? OldTime { get; private set; }
	}
}