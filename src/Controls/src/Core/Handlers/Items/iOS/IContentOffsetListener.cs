namespace Microsoft.Maui.Controls.Handlers.Items;

/// <summary>
/// Implemented by a collection view controller that needs to know of every change of the content offset of its
/// <see cref="MauiCollectionView"/> on Mac Catalyst, including one that no scroll view delegate, layout or view controller
/// callback reports, as when the window changes.
/// </summary>
internal interface IContentOffsetListener
{
	/// <summary>Called for every change of the content offset the view observes while it's in a window.</summary>
	void ContentOffsetChanged();
}
