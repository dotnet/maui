using Microsoft.UI.Xaml.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Specifics = Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.CollectionView;

namespace Microsoft.Maui.Controls.Platform
{
	public static class CollectionViewExtensions
	{
		public static void UpdateSingleSelectionFollowsFocus(this ListViewBase listView, CollectionView collectionView)
		{
			if (collectionView.IsSet(Specifics.SingleSelectionFollowsFocusProperty))
			{
				var singleSelectionFollowsFocus = collectionView.OnThisPlatform().GetSingleSelectionFollowsFocus();	
				
				listView.SingleSelectionFollowsFocus = singleSelectionFollowsFocus;
			}
		}
	}
}