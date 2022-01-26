using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TappedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.TappedEventArgs']/Docs" />
	public class TappedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/TappedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public TappedEventArgs(object parameter)
		{
			Parameter = parameter;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TappedEventArgs.xml" path="//Member[@MemberName='Parameter']/Docs" />
		public object Parameter { get; private set; }
	}
}