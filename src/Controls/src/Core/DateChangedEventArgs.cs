using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/DateChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.DateChangedEventArgs']/Docs" />
	public class DateChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/DateChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public DateChangedEventArgs(DateTime oldDate, DateTime newDate)
		{
			OldDate = oldDate;
			NewDate = newDate;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/DateChangedEventArgs.xml" path="//Member[@MemberName='NewDate']/Docs" />
		public DateTime NewDate { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/DateChangedEventArgs.xml" path="//Member[@MemberName='OldDate']/Docs" />
		public DateTime OldDate { get; private set; }
	}
}