using System;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public class SwipeBehaviorOnInvokedGallery : ContentPage
	{
		public SwipeBehaviorOnInvokedGallery()
		{
			Title = "SwipeBehaviorOnInvoked Gallery";

			var swipeLayout = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var swipeBehaviorOnInvokedLabel = new Label
			{
				FontSize = 10,
				Text = "SwipeBehaviorOnInvoked:"
			};

			swipeLayout.Children.Add(swipeBehaviorOnInvokedLabel);

			var swipeBehaviorOnInvokedItems = Enum.GetNames(typeof(SwipeBehaviorOnInvoked)).Select(s => s).ToList();

			var swipeBehaviorOnInvokedPicker = new Picker
			{
				ItemsSource = swipeBehaviorOnInvokedItems,
				SelectedIndex = 0
			};

			swipeLayout.Children.Add(swipeBehaviorOnInvokedPicker);

			var deleteSwipeItem = new SwipeItem { BackgroundColor = Colors.Red, Text = "Delete", IconImageSource = "calculator.png" };
			deleteSwipeItem.Invoked += (sender, e) => { DisplayAlert("SwipeView", "Delete Invoked", "Ok"); };

			var leftSwipeItems = new SwipeItems
			{
				deleteSwipeItem
			};

			leftSwipeItems.Mode = SwipeMode.Reveal;
			leftSwipeItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Auto;

			var swipeContent = new Grid
			{
				BackgroundColor = Colors.Gray
			};

			var swipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right (and tap the SwipeItem)"
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

			swipeBehaviorOnInvokedPicker.SelectedIndexChanged += (sender, e) =>
			{
				Enum.TryParse(swipeBehaviorOnInvokedPicker.SelectedItem.ToString(), out SwipeBehaviorOnInvoked swipeBehaviorOnInvoked);
				leftSwipeItems.SwipeBehaviorOnInvoked = swipeBehaviorOnInvoked;
			};

			Content = swipeLayout;
		}
	}
}