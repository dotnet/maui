using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/EffectiveFlowDirection.xml" path="Type[@FullName='Microsoft.Maui.Controls.EffectiveFlowDirection']/Docs/*" />
	[Flags]
	public enum EffectiveFlowDirection
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/EffectiveFlowDirection.xml" path="//Member[@MemberName='RightToLeft']/Docs/*" />
		RightToLeft = 1 << 0,
		/// <include file="../../docs/Microsoft.Maui.Controls/EffectiveFlowDirection.xml" path="//Member[@MemberName='Explicit']/Docs/*" />
		Explicit = 1 << 1
	}
}