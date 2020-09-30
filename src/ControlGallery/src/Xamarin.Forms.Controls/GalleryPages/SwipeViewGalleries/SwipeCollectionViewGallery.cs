using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public class SwipeCollectionViewGallery : ContentPage
	{
		public SwipeCollectionViewGallery()
		{
			Title = "CollectionView Galleries";
			Content = new StackLayout
			{
				Children =
				{
					GalleryBuilder.NavButton("Horizontal CollectionView Gallery", () => new SwipeHorizontalCollectionViewGallery(), Navigation),
					GalleryBuilder.NavButton("Vertical CollectionView Gallery", () => new SwipeVerticalCollectionViewGallery(), Navigation)
				}
			};
		}
	}
}