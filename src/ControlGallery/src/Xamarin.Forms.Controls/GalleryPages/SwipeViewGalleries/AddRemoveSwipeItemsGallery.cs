using System;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public class AddRemoveSwipeItemsGallery : ContentPage
	{
		public AddRemoveSwipeItemsGallery()
		{
			Title = "Add/Remove SwipeItems Gallery";

			var swipeLayout = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var buttonsLayout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal
			};

			var addButton = new Button
			{
				Text = "Add SwipeItem"
			};

			buttonsLayout.Children.Add(addButton);

			var removeButton = new Button
			{
				Text = "Remove SwipeItem"
			};

			buttonsLayout.Children.Add(removeButton);

			var swipeItemIconLabel = new Label
			{
				FontSize = 10,
				Text = "Choose SwipeItem Icon:"
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

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = leftSwipeItems,
				Content = swipeContent
			};

			swipeLayout.Children.Add(buttonsLayout);
			swipeLayout.Children.Add(swipeView);

			Content = swipeLayout;

			addButton.Clicked += (sender, e) =>
			{
				swipeView.Close();

				var swipeItemsCount = swipeView.LeftItems.Count;
				var random = new Random();

				var newSwipeItem = new SwipeItem
				{
					BackgroundColor = Color.FromRgb(random.Next(0, 255), random.Next(0, 255), random.Next(0, 255)),
					IconImageSource = "calculator.png",
					Text = $"SwipeItem{swipeItemsCount + 1}"
				};

				swipeView.LeftItems.Add(newSwipeItem);
			};

			removeButton.Clicked += (sender, e) =>
			{
				swipeView.Close();

				var swipeItemsCount = swipeView.LeftItems.Count;

				if (swipeItemsCount > 0)
				{
					var lastSwipeItem = swipeView.LeftItems.LastOrDefault();
					swipeView.LeftItems.Remove(lastSwipeItem);
				}
			};
		}
	}
}