using System;
using System.Linq;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Controls.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public class SwipeTransitionModeGallery : ContentPage
	{
		public SwipeTransitionModeGallery()
		{
			Title = "SwipeTransitionMode Gallery";

			var scroll = new ScrollView();

			var swipeLayout = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var instructions = new Label
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "This Gallery use a Platform Specific only available for Android and iOS."
			};

			swipeLayout.Children.Add(instructions);

			var swipeItemSwipeTransitionModeLabel = new Label
			{
				FontSize = 10,
				Text = "SwipeTransitionMode:"
			};

			swipeLayout.Children.Add(swipeItemSwipeTransitionModeLabel);

			var swipeItemSwipeTransitionModePicker = new Picker();

			var swipeTransitionModes = Enum.GetNames(typeof(PlatformConfiguration.AndroidSpecific.SwipeTransitionMode)).Select(t => t).ToList();

			swipeItemSwipeTransitionModePicker.ItemsSource = swipeTransitionModes;
			swipeItemSwipeTransitionModePicker.SelectedIndex = 0;   // Reveal

			swipeLayout.Children.Add(swipeItemSwipeTransitionModePicker);

			var deleteSwipeItem = new SwipeItem
			{
				BackgroundColor = Color.Red,
				IconImageSource = "calculator.png",
				Text = "Delete"
			};

			deleteSwipeItem.Invoked += (sender, e) => { DisplayAlert("SwipeView", "Delete Invoked", "Ok"); };

			var swipeItems = new SwipeItems { deleteSwipeItem };

			swipeItems.Mode = SwipeMode.Reveal;

			var leftSwipeContent = new Grid
			{
				BackgroundColor = Color.Gray
			};

			var leftSwipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right"
			};

			leftSwipeContent.Children.Add(leftSwipeLabel);

			var leftSwipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = swipeItems,
				Content = leftSwipeContent
			};

			swipeLayout.Children.Add(leftSwipeView);

			var rightSwipeContent = new Grid
			{
				BackgroundColor = Color.Gray
			};

			var rightSwipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Left"
			};

			rightSwipeContent.Children.Add(rightSwipeLabel);

			var rightSwipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				RightItems = swipeItems,
				Content = rightSwipeContent
			};

			swipeLayout.Children.Add(rightSwipeView);

			var topSwipeContent = new Grid
			{
				BackgroundColor = Color.Gray
			};

			var topSwipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Top"
			};

			topSwipeContent.Children.Add(topSwipeLabel);

			var topSwipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				BottomItems = swipeItems,
				Content = topSwipeContent
			};

			swipeLayout.Children.Add(topSwipeView);

			var bottomSwipeContent = new Grid
			{
				BackgroundColor = Color.Gray
			};

			var bottomSwipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Bottom"
			};

			bottomSwipeContent.Children.Add(bottomSwipeLabel);

			var bottomSwipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				TopItems = swipeItems,
				Content = bottomSwipeContent
			};

			swipeLayout.Children.Add(bottomSwipeView);

			swipeItemSwipeTransitionModePicker.SelectedIndexChanged += (sender, e) =>
			{
				var swipeTransitionMode = swipeItemSwipeTransitionModePicker.SelectedItem;

				switch (swipeTransitionMode)
				{
					case "Drag":
						leftSwipeView.On<Android>().SetSwipeTransitionMode(PlatformConfiguration.AndroidSpecific.SwipeTransitionMode.Drag);
						leftSwipeView.On<iOS>().SetSwipeTransitionMode(PlatformConfiguration.iOSSpecific.SwipeTransitionMode.Drag);

						rightSwipeView.On<Android>().SetSwipeTransitionMode(PlatformConfiguration.AndroidSpecific.SwipeTransitionMode.Drag);
						rightSwipeView.On<iOS>().SetSwipeTransitionMode(PlatformConfiguration.iOSSpecific.SwipeTransitionMode.Drag);

						topSwipeView.On<Android>().SetSwipeTransitionMode(PlatformConfiguration.AndroidSpecific.SwipeTransitionMode.Drag);
						topSwipeView.On<iOS>().SetSwipeTransitionMode(PlatformConfiguration.iOSSpecific.SwipeTransitionMode.Drag);

						bottomSwipeView.On<Android>().SetSwipeTransitionMode(PlatformConfiguration.AndroidSpecific.SwipeTransitionMode.Drag);
						bottomSwipeView.On<iOS>().SetSwipeTransitionMode(PlatformConfiguration.iOSSpecific.SwipeTransitionMode.Drag);
						break;
					case "Reveal":
						leftSwipeView.On<Android>().SetSwipeTransitionMode(PlatformConfiguration.AndroidSpecific.SwipeTransitionMode.Reveal);
						leftSwipeView.On<iOS>().SetSwipeTransitionMode(PlatformConfiguration.iOSSpecific.SwipeTransitionMode.Reveal);

						rightSwipeView.On<Android>().SetSwipeTransitionMode(PlatformConfiguration.AndroidSpecific.SwipeTransitionMode.Drag);
						rightSwipeView.On<iOS>().SetSwipeTransitionMode(PlatformConfiguration.iOSSpecific.SwipeTransitionMode.Drag);

						topSwipeView.On<Android>().SetSwipeTransitionMode(PlatformConfiguration.AndroidSpecific.SwipeTransitionMode.Drag);
						topSwipeView.On<iOS>().SetSwipeTransitionMode(PlatformConfiguration.iOSSpecific.SwipeTransitionMode.Drag);

						bottomSwipeView.On<Android>().SetSwipeTransitionMode(PlatformConfiguration.AndroidSpecific.SwipeTransitionMode.Drag);
						bottomSwipeView.On<iOS>().SetSwipeTransitionMode(PlatformConfiguration.iOSSpecific.SwipeTransitionMode.Drag);
						break;
				}
			};

			scroll.Content = swipeLayout;

			Content = scroll;
		}
	}
}