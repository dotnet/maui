#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
	/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class PageExtensions
	{
		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		/// <param name="page">Internal parameter for platform use.</param>
		/// <returns>For internal use by the Microsoft.Maui.Controls platform.</returns>
		public static Page AncestorToRoot(this Page page)
		{
			Element parent = page;
			while (!Application.IsApplicationOrWindowOrNull(parent.RealParent))
				parent = parent.RealParent;

			return parent as Page;
		}
	}
}
