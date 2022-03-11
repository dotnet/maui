using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ElementEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.ElementEventArgs']/Docs" />
	public class ElementEventArgs : EventArgs
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ElementEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public ElementEventArgs(Element element) => Element = element ?? throw new ArgumentNullException(nameof(element));

		/// <include file="../../docs/Microsoft.Maui.Controls/ElementEventArgs.xml" path="//Member[@MemberName='Element']/Docs" />
		public Element Element { get; private set; }
	}
}