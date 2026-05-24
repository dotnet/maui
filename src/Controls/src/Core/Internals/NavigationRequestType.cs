using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>Specifies the type of navigation operation.</summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public enum NavigationRequestType
	{
		/// <summary>Unknown navigation type.</summary>
		Unknown = 0,
		/// <summary>Push a page onto the navigation stack.</summary>
		Push,
		/// <summary>Pop a page from the navigation stack.</summary>
		Pop,
		/// <summary>Pop all pages to root.</summary>
		PopToRoot,
		/// <summary>Insert a page into the navigation stack.</summary>
		Insert,
		/// <summary>Remove a page from the navigation stack.</summary>
		Remove,
	}
}