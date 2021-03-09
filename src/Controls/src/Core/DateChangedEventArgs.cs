using System;

namespace Microsoft.Maui.Controls
{
	public class DateChangedEventArgs : EventArgs
	{
		public DateChangedEventArgs(DateTime oldDate, DateTime newDate)
		{
			OldDate = oldDate;
			NewDate = newDate;
		}

		public DateTime NewDate { get; private set; }

		public DateTime OldDate { get; private set; }
	}
}