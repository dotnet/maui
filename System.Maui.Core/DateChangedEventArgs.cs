using System;

namespace Xamarin.Forms
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