using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ButtonsMask.xml" path="Type[@FullName='Microsoft.Maui.Controls.ButtonsMask']/Docs" />
	[Flags]
	public enum ButtonsMask
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ButtonsMask.xml" path="//Member[@MemberName='Primary']/Docs" />
		Primary = 1 << 0,
		/// <include file="../../docs/Microsoft.Maui.Controls/ButtonsMask.xml" path="//Member[@MemberName='Secondary']/Docs" />
		Secondary = 1 << 1
	}
}
