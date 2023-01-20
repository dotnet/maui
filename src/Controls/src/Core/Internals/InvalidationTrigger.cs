using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/InvalidationTrigger.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.InvalidationTrigger']/Docs/*" />
	[Flags]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public enum InvalidationTrigger
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/InvalidationTrigger.xml" path="//Member[@MemberName='Undefined']/Docs/*" />
		Undefined = 0,
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/InvalidationTrigger.xml" path="//Member[@MemberName='MeasureChanged']/Docs/*" />
		MeasureChanged = 1 << 0,
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/InvalidationTrigger.xml" path="//Member[@MemberName='HorizontalOptionsChanged']/Docs/*" />
		HorizontalOptionsChanged = 1 << 1,
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/InvalidationTrigger.xml" path="//Member[@MemberName='VerticalOptionsChanged']/Docs/*" />
		VerticalOptionsChanged = 1 << 2,
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/InvalidationTrigger.xml" path="//Member[@MemberName='SizeRequestChanged']/Docs/*" />
		SizeRequestChanged = 1 << 3,
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/InvalidationTrigger.xml" path="//Member[@MemberName='RendererReady']/Docs/*" />
		RendererReady = 1 << 4,
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/InvalidationTrigger.xml" path="//Member[@MemberName='MarginChanged']/Docs/*" />
		MarginChanged = 1 << 5
	}
}