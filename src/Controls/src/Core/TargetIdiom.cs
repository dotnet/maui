using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/TargetIdiom.xml" path="Type[@FullName='Microsoft.Maui.Controls.TargetIdiom']/Docs/*" />
	[Obsolete("Use Microsoft.Maui.Devices.DeviceIdiom instead.")]
	public enum TargetIdiom
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/TargetIdiom.xml" path="//Member[@MemberName='Unsupported']/Docs/*" />
		Unsupported,
		/// <include file="../../docs/Microsoft.Maui.Controls/TargetIdiom.xml" path="//Member[@MemberName='Phone']/Docs/*" />
		Phone,
		/// <include file="../../docs/Microsoft.Maui.Controls/TargetIdiom.xml" path="//Member[@MemberName='Tablet']/Docs/*" />
		Tablet,
		/// <include file="../../docs/Microsoft.Maui.Controls/TargetIdiom.xml" path="//Member[@MemberName='Desktop']/Docs/*" />
		Desktop,
		/// <include file="../../docs/Microsoft.Maui.Controls/TargetIdiom.xml" path="//Member[@MemberName='TV']/Docs/*" />
		TV,
		/// <include file="../../docs/Microsoft.Maui.Controls/TargetIdiom.xml" path="//Member[@MemberName='Watch']/Docs/*" />
		Watch
	}
}
