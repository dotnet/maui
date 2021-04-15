using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.SwipeViewGalleries
{
	public class SwipeItemIconGallery : ContentPage
	{
		public SwipeItemIconGallery()
		{
			Title = "SwipeItem Icon Gallery";

			var scroll = new ScrollView();

			var swipeLayout = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var fileSwipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				IconImageSource = "calculator.png",
				Text = "File"
			};

			fileSwipeItem.Invoked += (sender, e) => { DisplayAlert("SwipeView", "File Invoked", "Ok"); };

			var fileSwipeItems = new SwipeItems { fileSwipeItem };

			fileSwipeItems.Mode = SwipeMode.Reveal;

			var fileSwipeContent = new Grid
			{
				BackgroundColor = Colors.Gray
			};

			var fileSwipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right (File)"
			};

			fileSwipeContent.Children.Add(fileSwipeLabel);

			var fileSwipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = fileSwipeItems,
				Content = fileSwipeContent
			};

			swipeLayout.Children.Add(fileSwipeView);

			var urlSwipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				IconImageSource = "https://image.flaticon.com/icons/png/512/61/61848.png",
				Text = "Url"
			};

			urlSwipeItem.Invoked += (sender, e) => { DisplayAlert("SwipeView", "Url Invoked", "Ok"); };

			var urlSwipeItems = new SwipeItems { urlSwipeItem };

			urlSwipeItems.Mode = SwipeMode.Reveal;

			var urlSwipeContent = new Grid
			{
				BackgroundColor = Colors.Gray
			};

			var urlSwipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right (Url)"
			};

			urlSwipeContent.Children.Add(urlSwipeLabel);

			var urlSwipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = urlSwipeItems,
				Content = urlSwipeContent
			};

			swipeLayout.Children.Add(urlSwipeView);

			var fontFamily = string.Empty;

			switch (Device.RuntimePlatform)
			{
				case Device.iOS:
					fontFamily = "Ionicons";
					break;
				case Device.UWP:
					fontFamily = "Assets/Fonts/ionicons.ttf#ionicons";
					break;
				case Device.Android:
				default:
					fontFamily = "fonts/ionicons.ttf#";
					break;
			}

			var fontSwipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				IconImageSource = new FontImageSource
				{
					Glyph = "\uf101",
					FontFamily = fontFamily,
					Size = 16
				},
				Text = "Font"
			};

			fontSwipeItem.Invoked += (sender, e) => { DisplayAlert("SwipeView", "Font Invoked", "Ok"); };

			var fontSwipeItems = new SwipeItems { fontSwipeItem };

			fontSwipeItems.Mode = SwipeMode.Reveal;

			var fontSwipeContent = new Grid
			{
				BackgroundColor = Colors.Gray
			};

			var fontSwipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right (Font)"
			};

			fontSwipeContent.Children.Add(fontSwipeLabel);

			var fontSwipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = fontSwipeItems,
				Content = fontSwipeContent
			};

			swipeLayout.Children.Add(fontSwipeView);

			scroll.Content = swipeLayout;

			Content = scroll;
		}
	}
}