#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/SwipedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.SwipedEventArgs']/Docs/*" />
	public class SwipedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/SwipedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public SwipedEventArgs(object parameter, SwipeDirection direction)
		{
			Parameter = parameter;
			Direction = direction;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipedEventArgs.xml" path="//Member[@MemberName='Parameter']/Docs/*" />
		public object Parameter { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/SwipedEventArgs.xml" path="//Member[@MemberName='Direction']/Docs/*" />
		public SwipeDirection Direction { get; private set; }
	}
}