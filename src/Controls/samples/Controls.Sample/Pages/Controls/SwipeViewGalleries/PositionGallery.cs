using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public class PositionGallery : ContentPage
	{
		public PositionGallery()
		{
			Title = "Position Galleries";
			Content = new StackLayout
			{
				Children =
				{
					GalleryBuilder.NavButton("SwipeItem Position Gallery", () => new SwipeItemPositionGallery(), Navigation),
					GalleryBuilder.NavButton("SwipeItemView Position Gallery", () => new SwipeItemViewPositionGallery(), Navigation)
				}
			};
		}
	}
}