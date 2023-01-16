#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TextChangedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.TextChangedEventArgs']/Docs/*" />
	public class TextChangedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/TextChangedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public TextChangedEventArgs(string oldTextValue, string newTextValue)
		{
			OldTextValue = oldTextValue;
			NewTextValue = newTextValue;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/TextChangedEventArgs.xml" path="//Member[@MemberName='NewTextValue']/Docs/*" />
		public string NewTextValue { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/TextChangedEventArgs.xml" path="//Member[@MemberName='OldTextValue']/Docs/*" />
		public string OldTextValue { get; private set; }
	}
}