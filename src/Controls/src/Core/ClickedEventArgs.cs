#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ClickedEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.ClickedEventArgs']/Docs/*" />
	public class ClickedEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ClickedEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ClickedEventArgs(ButtonsMask buttons, object commandParameter)
		{
			Buttons = buttons;
			Parameter = commandParameter;
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ClickedEventArgs.xml" path="//Member[@MemberName='Buttons']/Docs/*" />
		public ButtonsMask Buttons { get; private set; }

		/// <include file="../../docs/Microsoft.Maui.Controls/ClickedEventArgs.xml" path="//Member[@MemberName='Parameter']/Docs/*" />
		public object Parameter { get; private set; }
	}
}