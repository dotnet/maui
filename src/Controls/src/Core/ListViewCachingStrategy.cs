using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ListViewCachingStrategy.xml" path="Type[@FullName='Microsoft.Maui.Controls.ListViewCachingStrategy']/Docs" />
	[Flags]
	public enum ListViewCachingStrategy
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ListViewCachingStrategy.xml" path="//Member[@MemberName='RetainElement']/Docs" />
		RetainElement = 0,
		/// <include file="../../docs/Microsoft.Maui.Controls/ListViewCachingStrategy.xml" path="//Member[@MemberName='RecycleElement']/Docs" />
		RecycleElement = 1 << 0,
		/// <include file="../../docs/Microsoft.Maui.Controls/ListViewCachingStrategy.xml" path="//Member[@MemberName='RecycleElementAndDataTemplate']/Docs" />
		RecycleElementAndDataTemplate = RecycleElement | 1 << 1,
	}
}