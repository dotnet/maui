using System;

namespace System.Maui
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