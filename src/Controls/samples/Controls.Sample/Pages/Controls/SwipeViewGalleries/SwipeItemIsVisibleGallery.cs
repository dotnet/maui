﻿using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public class SwipeItemIsVisibleGallery : ContentPage
	{
		public SwipeItemIsVisibleGallery()
		{
			Title = "SwipeItem IsVisible Gallery";

			var swipeLayout = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var instructions = new Label
			{
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "RemainOpen is used as SwipeBehaviorOnInvoked, tapping on a SwipeItem will not close it."
			};

			swipeLayout.Children.Add(instructions);

			var closeButton = new Button
			{
				Text = "Close SwipeView"
			};

			swipeLayout.Children.Add(closeButton);

			var swipeItem1 = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				IconImageSource = "calculator.png",
				Text = "Test 1",
				IsVisible = false
			};

			swipeItem1.Invoked += (sender, e) => { DisplayAlertAsync("SwipeView", "Test 1 Invoked", "Ok"); };

			var swipeItem2 = new SwipeItem
			{
				BackgroundColor = Colors.Orange,
				IconImageSource = "coffee.png",
				Text = "Test 2"
			};

			swipeItem2.Invoked += (sender, e) => { DisplayAlertAsync("SwipeView", "Test 2 Invoked", "Ok"); };

			var swipeItems = new SwipeItems { swipeItem1, swipeItem2 };

			swipeItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen;
			swipeItems.Mode = SwipeMode.Reveal;

			var swipeContent = new Grid
			{
				BackgroundColor = Colors.Gray
			};

			var fileSwipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right"
			};

			swipeContent.Children.Add(fileSwipeLabel);

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = swipeContent
			};

			swipeLayout.Children.Add(swipeView);


			var swipeItem1Layout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal
			};

			var swipeItem1Button = new Button
			{
				Text = "Show SwipeItem 1",
				WidthRequest = 250,
				VerticalOptions = LayoutOptions.Center
			};

			var swipeItem1Label = new Label
			{
				Text = "SwipeItem 1 is hidden",
				VerticalOptions = LayoutOptions.Center
			};

			swipeItem1Layout.Children.Add(swipeItem1Button);
			swipeItem1Layout.Children.Add(swipeItem1Label);

			swipeLayout.Children.Add(swipeItem1Layout);

			var swipeItem2Layout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal
			};

			var swipeItem2Button = new Button
			{
				Text = "Hide SwipeItem 2",
				WidthRequest = 250,
				VerticalOptions = LayoutOptions.Center
			};

			var swipeItem2Label = new Label
			{
				Text = "SwipeItem 2 is visible",
				VerticalOptions = LayoutOptions.Center
			};

			swipeItem2Layout.Children.Add(swipeItem2Button);
			swipeItem2Layout.Children.Add(swipeItem2Label);

			swipeLayout.Children.Add(swipeItem2Layout);

			Content = swipeLayout;

			closeButton.Clicked += (sender, e) =>
			{
				swipeView.Close();
			};

			swipeItem1Button.Clicked += (sender, args) =>
			{
				swipeItem1.IsVisible = !swipeItem1.IsVisible;

				if (swipeItem1.IsVisible)
				{
					swipeItem1Button.Text = "Hide SwipeItem 1";
					swipeItem1Label.Text = "SwipeItem 1 is visible";
				}
				else
				{
					swipeItem1Button.Text = "Show SwipeItem 1";
					swipeItem1Label.Text = "SwipeItem 1 is hidden";
				}
			};

			swipeItem2Button.Clicked += (sender, args) =>
			{
				swipeItem2.IsVisible = !swipeItem2.IsVisible;

				if (swipeItem2.IsVisible)
				{
					swipeItem2Button.Text = "Hide SwipeItem 2";
					swipeItem2Label.Text = "SwipeItem 2 is visible";
				}
				else
				{
					swipeItem2Button.Text = "Show SwipeItem 2";
					swipeItem2Label.Text = "SwipeItem 2 is hidden";
				}
			};
		}
	}
}