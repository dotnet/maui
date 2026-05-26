using System.Collections.Generic;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public class CustomizeSwipeItemGallery : ContentPage
	{
		public CustomizeSwipeItemGallery()
		{
			Title = "Customize SwipeItem Gallery";

			var swipeLayout = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var swipeItemTextLabel = new Label
			{
				FontSize = 10,
				Text = "SwipeItem Text:"
			};

			swipeLayout.Children.Add(swipeItemTextLabel);

			var swipeItemTextEntry = new Entry
			{
				Text = "Delete",
				Placeholder = "SwipeItem Text"
			};

			swipeLayout.Children.Add(swipeItemTextEntry);

			var swipeItemBackgroundColorLabel = new Label
			{
				FontSize = 10,
				Text = "Choose SwipeItem BackgroundColor:"
			};

			swipeLayout.Children.Add(swipeItemBackgroundColorLabel);

			var swipeItemBackgroundColorPicker = new Picker();
			var colors = new List<string> { "#FFFFFF", "#FF0000", "#00FF00", "#0000FF", "#000000" };
			swipeItemBackgroundColorPicker.ItemsSource = colors;
			swipeItemBackgroundColorPicker.SelectedItem = colors[1];

			swipeLayout.Children.Add(swipeItemBackgroundColorPicker);


			var swipeItemIconLabel = new Label
			{
				FontSize = 10,
				Text = "Choose SwipeItem Icon:"
			};

			swipeLayout.Children.Add(swipeItemIconLabel);

			var swipeItemIconPicker = new Picker();
			var icons = new List<string> { "bank.png", "calculator.png", "coffee.png" };
			swipeItemIconPicker.ItemsSource = icons;
			swipeItemIconPicker.SelectedItem = icons[1];

			swipeLayout.Children.Add(swipeItemIconPicker);

			var deleteSwipeItem = new SwipeItem
			{
				BackgroundColor = Color.FromArgb(colors[swipeItemBackgroundColorPicker.SelectedIndex]),
				IconImageSource = swipeItemIconPicker.SelectedItem.ToString(),
				Text = swipeItemTextEntry.Text
			};

			deleteSwipeItem.Invoked += (sender, e) => { DisplayAlertAsync("SwipeView", "Delete Invoked", "Ok"); };

			var leftSwipeItems = new SwipeItems
			{
				deleteSwipeItem
			};

			leftSwipeItems.Mode = SwipeMode.Execute;

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
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = leftSwipeItems,
				Content = swipeContent
			};

			swipeLayout.Children.Add(swipeView);

			swipeItemTextEntry.TextChanged += (sender, e) =>
			{
				deleteSwipeItem.Text = swipeItemTextEntry.Text;
			};

			swipeItemBackgroundColorPicker.SelectedIndexChanged += (sender, e) =>
			{
				deleteSwipeItem.BackgroundColor = Color.FromArgb(colors[swipeItemBackgroundColorPicker.SelectedIndex]);
			};

			swipeItemIconPicker.SelectedIndexChanged += (sender, e) =>
			{
				deleteSwipeItem.IconImageSource = swipeItemIconPicker.SelectedItem.ToString();
			};

			Content = swipeLayout;
		}
	}
}