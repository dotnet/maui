#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/PageExtensions.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.PageExtensions']/Docs/*" />
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class PageExtensions
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/PageExtensions.xml" path="//Member[@MemberName='AncestorToRoot']/Docs/*" />
		public static Page AncestorToRoot(this Page page)
		{
			Element parent = page;
			while (!Application.IsApplicationOrWindowOrNull(parent.RealParent))
				parent = parent.RealParent;

			return parent as Page;
		}
	}
}
