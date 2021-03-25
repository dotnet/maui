using System;
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.SwipeViewGalleries
{
	public class NoIconTextSwipeItemGallery : ContentPage
	{
		public NoIconTextSwipeItemGallery()
		{
			Title = "No Icon or Text SwipeItem Gallery";

			var swipeLayout = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var noIconSwipeItem = new SwipeItem
			{
				BackgroundColor = Color.Red,
				Text = "File"
			};

			var noTextSwipeItem = new SwipeItem
			{
				BackgroundColor = Color.BlueViolet,
				IconImageSource = "calculator.png"
			};

			var swipeItems = new SwipeItems { noIconSwipeItem, noTextSwipeItem };

			swipeItems.Mode = SwipeMode.Reveal;

			var swipeContent = new Grid
			{
				BackgroundColor = Color.Gray
			};

			var swipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right (File)"
			};

			swipeContent.Children.Add(swipeLabel);

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = swipeContent
			};

			swipeLayout.Children.Add(swipeView);

			Content = swipeLayout;
		}
	}
}