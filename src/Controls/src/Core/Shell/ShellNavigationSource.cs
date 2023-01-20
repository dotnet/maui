using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigationSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.ShellNavigationSource']/Docs/*" />
	public enum ShellNavigationSource
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigationSource.xml" path="//Member[@MemberName='Unknown']/Docs/*" />
		Unknown = 0,
		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigationSource.xml" path="//Member[@MemberName='Push']/Docs/*" />
		Push,
		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigationSource.xml" path="//Member[@MemberName='Pop']/Docs/*" />
		Pop,
		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigationSource.xml" path="//Member[@MemberName='PopToRoot']/Docs/*" />
		PopToRoot,
		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigationSource.xml" path="//Member[@MemberName='Insert']/Docs/*" />
		Insert,
		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigationSource.xml" path="//Member[@MemberName='Remove']/Docs/*" />
		Remove,
		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigationSource.xml" path="//Member[@MemberName='ShellItemChanged']/Docs/*" />
		ShellItemChanged,
		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigationSource.xml" path="//Member[@MemberName='ShellSectionChanged']/Docs/*" />
		ShellSectionChanged,
		/// <include file="../../../docs/Microsoft.Maui.Controls/ShellNavigationSource.xml" path="//Member[@MemberName='ShellContentChanged']/Docs/*" />
		ShellContentChanged,
	}
}