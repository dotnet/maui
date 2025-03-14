using Microsoft.Maui.Controls.Handlers.Items;

namespace Microsoft.Maui.Controls
{
	public partial class CollectionView
	{
		public static void MapSingleSelectionFollowsFocus(CollectionViewHandler handler, CollectionView collectionView)
		{
			Platform.CollectionViewExtensions.UpdateSingleSelectionFollowsFocus(handler.PlatformView, collectionView);
		}
	}
}