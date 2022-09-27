using System.Collections.Generic;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a menu item that displays a sub-menu in a MenuFlyout view.
	/// </summary>
	public interface IMenuFlyoutSubItem : IMenuFlyoutItem, IList<IMenuElement>
	{

	}
}