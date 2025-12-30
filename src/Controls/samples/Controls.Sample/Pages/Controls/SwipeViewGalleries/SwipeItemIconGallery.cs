using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
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
				IconImageSource = "coffee.png",
				Text = "File"
			};

			fileSwipeItem.Invoked += (sender, e) => { DisplayAlertAsync("SwipeView", "File Invoked", "Ok"); };

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

			urlSwipeItem.Invoked += (sender, e) => { DisplayAlertAsync("SwipeView", "Url Invoked", "Ok"); };

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

			var fontFamily = DefaultFontFamily();

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

			fontSwipeItem.Invoked += (sender, e) => { DisplayAlertAsync("SwipeView", "Font Invoked", "Ok"); };

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

		static string DefaultFontFamily()
		{
			var fontFamily = "";
			if (DeviceInfo.Platform == DevicePlatform.iOS)
				fontFamily = "Ionicons";
			else if (DeviceInfo.Platform == DevicePlatform.WinUI)
				fontFamily = "Assets/Fonts/ionicons.ttf#ionicons";
			else
				fontFamily = "fonts/ionicons.ttf#";
			return fontFamily;
		}
	}
}