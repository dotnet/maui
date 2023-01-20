using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ViewState.xml" path="Type[@FullName='Microsoft.Maui.Controls.ViewState']/Docs/*" />
	[Flags]
	public enum ViewState
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ViewState.xml" path="//Member[@MemberName='Default']/Docs/*" />
		Default = 0,
		/// <include file="../../docs/Microsoft.Maui.Controls/ViewState.xml" path="//Member[@MemberName='Prelight']/Docs/*" />
		Prelight = 1,
		/// <include file="../../docs/Microsoft.Maui.Controls/ViewState.xml" path="//Member[@MemberName='Pressed']/Docs/*" />
		Pressed = 1 << 1
	}
}