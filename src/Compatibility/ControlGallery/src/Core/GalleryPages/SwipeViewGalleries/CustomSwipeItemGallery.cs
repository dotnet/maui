using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public class CustomSwipeItemGallery : ContentPage
	{
		public CustomSwipeItemGallery()
		{
			Title = "CustomSwipeItem Galleries";
			var layout = new StackLayout
			{
				Children =
				{
					GalleryBuilder.NavButton("Customize SwipeItem Gallery", () => new CustomizeSwipeItemGallery(), Navigation),
					GalleryBuilder.NavButton("No Icon or Text SwipeItem Gallery", () => new NoIconTextSwipeItemGallery(), Navigation)
				}
			};

			if (Device.RuntimePlatform != Device.WinUI)
			{
				layout.Children.Add(GalleryBuilder.NavButton("SwipeItemView Gallery", () => new CustomSwipeItemViewGallery(), Navigation));
				layout.Children.Add(GalleryBuilder.NavButton("CustomSwipeItem Size Gallery", () => new CustomSizeSwipeViewGallery(), Navigation));
			}

			Content = layout;
		}
	}
}