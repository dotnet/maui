using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/DependencyFetchTarget.xml" path="Type[@FullName='Microsoft.Maui.Controls.DependencyFetchTarget']/Docs/*" />
	[Obsolete("Use the service collection in the MauiAppBuilder instead. This will be removed in a future release.")]
	public enum DependencyFetchTarget
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/DependencyFetchTarget.xml" path="//Member[@MemberName='GlobalInstance']/Docs/*" />
		GlobalInstance,
		/// <include file="../../docs/Microsoft.Maui.Controls/DependencyFetchTarget.xml" path="//Member[@MemberName='NewInstance']/Docs/*" />
		NewInstance
	}
}
