#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>For internal use by the Microsoft.Maui.Controls platform.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class PageExtensions
	{
		/// <summary>For internal use by the Microsoft.Maui.Controls platform.</summary>
		/// <param name="page">For internal use by the Microsoft.Maui.Controls platform.</param>
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
