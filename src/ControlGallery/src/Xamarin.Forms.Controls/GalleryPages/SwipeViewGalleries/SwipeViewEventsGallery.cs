using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public class SwipeViewEventsGallery : ContentPage
	{
		public SwipeViewEventsGallery()
		{
			Title = "SwipeView Events Gallery";

			var swipeLayout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				},
				Margin = new Thickness(12)
			};

			var deleteSwipeItem = new SwipeItem
			{
				BackgroundColor = Color.Orange,
				IconImageSource = "calculator.png",
				Text = "SwipeItem1"
			};

			deleteSwipeItem.Invoked += (sender, e) => { DisplayAlert("SwipeView", "Delete Invoked", "Ok"); };

			var leftSwipeItems = new SwipeItems
			{
				deleteSwipeItem
			};

			leftSwipeItems.Mode = SwipeMode.Reveal;

			var swipeContent = new Grid
			{
				BackgroundColor = Color.Gray
			};

			var swipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right"
			};

			swipeContent.Children.Add(swipeLabel);

			var scroll = new ScrollView();
			var eventsInfo = new Label();
			scroll.Content = eventsInfo;

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = leftSwipeItems,
				Content = swipeContent
			};

			swipeLayout.Children.Add(swipeView, 0, 0);
			swipeLayout.Children.Add(scroll, 0, 1);

			Content = swipeLayout;

			swipeView.SwipeStarted += (sender, e) =>
			{
				eventsInfo.Text += $"SwipeStarted - Direction:{e.SwipeDirection}" + Environment.NewLine;
			};

			swipeView.SwipeChanging += (sender, e) =>
			{
				eventsInfo.Text += $"SwipeChanging - Direction:{e.SwipeDirection}, Offset:{e.Offset}" + Environment.NewLine;
			};

			swipeView.SwipeEnded += (sender, e) =>
			{
				eventsInfo.Text += $"SwipeEnded - Direction:{e.SwipeDirection}, IsOpen: {e.IsOpen}" + Environment.NewLine;
			};
		}
	}
}