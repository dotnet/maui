#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Event arguments for <see cref="E:Microsoft.Maui.Controls.TimePicker.TimeSelected" /> event.</summary>
	/// <remarks>To be added.</remarks>
	public class TimeChangedEventArgs : EventArgs
	{
		/// <summary>Creates a new <see cref="T:Microsoft.Maui.Controls.TimeChangedEventArgs" /> object that represents a change from <paramref name="oldTime" /> to <paramref name="newTime" />.</summary>
		/// <param name="oldTime"></param>
		/// <param name="newTime"></param>
		/// <remarks>To be added.</remarks>
		public TimeChangedEventArgs(TimeSpan? oldTime, TimeSpan? newTime)
		{
			OldTime = oldTime;
			NewTime = newTime;
		}

		/// <summary>The time that the user entered.</summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public TimeSpan? NewTime { get; private set; }

		/// <summary>The time that was on the element at the time that the user selected it.</summary>
		/// <value>To be added.</value>
		/// <remarks>To be added.</remarks>
		public TimeSpan? OldTime { get; private set; }
	}
}