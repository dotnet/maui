using System.Collections.Generic;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public class SwipeItemViewIsEnabledGallery : ContentPage
	{
		public SwipeItemViewIsEnabledGallery()
		{
			Title = "SwipeItemView IsEnabled Gallery";

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

			var swipeItemContent = new StackLayout();

			var monkeyList = new List<string>
			{
				"Baboon",
				"Capuchin Monkey",
				"Blue Monkey",
				"Squirrel Monkey",
				"Golden Lion Tamarin",
				"Howler Monkey",
				"Japanese Macaque"
			};

			var monkeyPicker = new Picker { Title = "Select a monkey" };

			monkeyPicker.ItemsSource = monkeyList;

			var monkeyButton = new Button
			{
				Text = "Get Selected Monkey"
			};

			monkeyButton.Clicked += (sender, args) =>
			{
				var selectedMonkey = monkeyPicker.SelectedItem ?? "None";
				DisplayAlert("Selected Monkey", $"{selectedMonkey}", "Ok");
			};

			swipeItemContent.Children.Add(monkeyPicker);
			swipeItemContent.Children.Add(monkeyButton);

			var swipeItem = new SwipeItemView
			{
				BackgroundColor = Colors.White,
				Content = swipeItemContent
			};

			var swipeItems = new SwipeItems { swipeItem };

			swipeItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen;
			swipeItems.Mode = SwipeMode.Reveal;

			var swipeContent = new Grid
			{
				BackgroundColor = Colors.Gray
			};

			var swipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right"
			};

			swipeContent.Children.Add(swipeLabel);

			var swipeView = new SwipeView
			{
				HeightRequest = 120,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = swipeContent
			};

			swipeLayout.Children.Add(swipeView);

			var swipeItemLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal
			};

			var swipeItemButton = new Button
			{
				Text = "Disable SwipeItemView",
				WidthRequest = 250,
				VerticalOptions = LayoutOptions.Center
			};

			var swipeItemLabel = new Label
			{
				Text = "SwipeItemView is enabled"
			};

			swipeItemLayout.Children.Add(swipeItemButton);
			swipeItemLayout.Children.Add(swipeItemLabel);

			swipeLayout.Children.Add(swipeItemLayout);

			Content = swipeLayout;

			closeButton.Clicked += (sender, e) =>
			{
				swipeView.Close();
			};

			swipeItemButton.Clicked += (sender, args) =>
			{
				swipeItem.IsEnabled = !swipeItem.IsEnabled;

				if (swipeItem.IsEnabled)
				{
					swipeItemButton.Text = "Disable SwipeItemView";
					swipeItemLabel.Text = "SwipeItemView is enabled";
				}
				else
				{
					swipeItemButton.Text = "Enable SwipeItemView";
					swipeItemLabel.Text = "SwipeItemView is disabled";
				}
			};
		}
	}
}