using System.Diagnostics;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	public class OpenCloseSwipeGallery : ContentPage
	{
		public OpenCloseSwipeGallery()
		{
			Title = "Open/Close SwipeView Gallery";

			var swipeLayout = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var openButton = new Button
			{
				HorizontalOptions = LayoutOptions.Fill,
				Text = "Open SwipeView"
			};

			var closeButton = new Button
			{
				HorizontalOptions = LayoutOptions.Fill,
				Text = "Close SwipeView"
			};

			var animatedLayout = new StackLayout
			{
				HorizontalOptions = LayoutOptions.Start,
				Orientation = StackOrientation.Horizontal
			};

			var animatedCheckBox = new CheckBox
			{
				IsChecked = true,
				VerticalOptions = LayoutOptions.Center
			};

			animatedLayout.Children.Add(animatedCheckBox);

			var animatedLabel = new Label
			{
				Text = "Animated",
				VerticalOptions = LayoutOptions.Center
			};

			animatedLayout.Children.Add(animatedLabel);

			swipeLayout.Children.Add(animatedLayout);
			swipeLayout.Children.Add(openButton);
			swipeLayout.Children.Add(closeButton);

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				IconImageSource = "calculator.png",
				Text = "File"
			};

			swipeItem.Invoked += (sender, e) => { DisplayAlertAsync("SwipeView", "SwipeItem Invoked", "Ok"); };

			var swipeItems1 = new SwipeItems { swipeItem };

			swipeItems1.Mode = SwipeMode.Reveal;

			var swipeContent1 = new Grid
			{
				BackgroundColor = Colors.Gray
			};

			var swipeContentText1 = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right (SwipeItem)"
			};

			swipeContent1.Children.Add(swipeContentText1);

			var swipeView1 = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems1,
				Content = swipeContent1
			};

			var infoLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				TextColor = Colors.White,
				Text = "View"
			};

			var swipeItemContent = new Grid
			{
				BackgroundColor = Colors.Red,
			};

			swipeItemContent.Children.Add(infoLabel);

			var swipeItemView = new SwipeItemView
			{
				Content = swipeItemContent
			};

			swipeItemView.Invoked += (sender, e) => { DisplayAlertAsync("SwipeView", "SwipeItemView Invoked", "Ok"); };

			var swipeItems2 = new SwipeItems { swipeItemView };

			swipeItems2.Mode = SwipeMode.Reveal;

			var swipeContent2 = new Grid
			{
				BackgroundColor = Colors.Gray
			};

			var swipeContentText2 = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right (SwipeItemView)"
			};

			swipeContent2.Children.Add(swipeContentText2);

			var swipeView2 = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems2,
				Content = swipeContent2
			};

			swipeLayout.Children.Add(swipeView1);
			swipeLayout.Children.Add(swipeView2);

			Content = swipeLayout;

			openButton.Clicked += (sender, e) =>
			{
				bool animated = animatedCheckBox.IsChecked;
				swipeView1.Open(OpenSwipeItem.LeftItems, animated);
				swipeView2.Open(OpenSwipeItem.LeftItems, animated);

				Debug.WriteLine($"swipeView1 IsOpen:{((ISwipeView)swipeView1).IsOpen}, swipeView2 IsOpen:{((ISwipeView)swipeView2).IsOpen}");
			};

			closeButton.Clicked += (sender, e) =>
			{
				bool animated = animatedCheckBox.IsChecked;
				swipeView1.Close(animated);
				swipeView2.Close(animated);

				Debug.WriteLine($"swipeView1 IsOpen:{((ISwipeView)swipeView1).IsOpen}, swipeView2 IsOpen:{((ISwipeView)swipeView2).IsOpen}");
			};
		}
	}
}