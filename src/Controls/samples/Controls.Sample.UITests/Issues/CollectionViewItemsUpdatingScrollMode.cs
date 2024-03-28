using System.Linq;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	// KeepScrollOffset (src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewItemsUpdatingScrollMode.cs)
	// KeepLastItemInView(src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewItemsUpdatingScrollMode.cs)
	[Issue(IssueTracker.None, 8888888, "CollectionView ItemsUpdatingScrollMode", PlatformAffected.All)]
	public class CollectionViewItemsUpdatingScrollMode : ContentPage
	{
		public CollectionViewItemsUpdatingScrollMode()
		{
			Navigation.PushAsync(new CollectionViewGalleries.ScrollModeGalleries.ScrollModeTestGallery());	
		}
	}
}