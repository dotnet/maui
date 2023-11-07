#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TimeChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.TimeChangedEventArgs']/Docs/*" />
	public class TimeChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/TimeChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public TimeChangedEventArgs(TimeSpan oldTime, TimeSpan newTime)
		{
			OldTime = oldTime;
			NewTime = newTime;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TimeChangedEventArgs.xml" path="//Member[@MemberName='NewTime']/Docs/*" />
		public TimeSpan NewTime { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/TimeChangedEventArgs.xml" path="//Member[@MemberName='OldTime']/Docs/*" />
		public TimeSpan OldTime { get; private set; }
	}
}