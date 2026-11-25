namespace Microsoft.Maui.Controls.Handlers.Items;

/// <summary>
/// Implemented by a collection view controller that needs to know when its items source has UIKit update the collection
/// view for a change in the items: UIKit lays the view out for the change as it does, and can move the content offset
/// before it updates the content size.
/// </summary>
internal interface IItemsUpdateObserver
{
	/// <summary>Called before the items source has UIKit update the collection view.</summary>
	void ItemsUpdating();

	/// <summary>Called once UIKit has updated the collection view.</summary>
	void ItemsUpdated();
}
