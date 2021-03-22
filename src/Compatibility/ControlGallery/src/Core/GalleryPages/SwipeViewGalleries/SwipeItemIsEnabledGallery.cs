using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public class SwipeItemIsEnabledGallery : ContentPage
	{
		public SwipeItemIsEnabledGallery()
		{
			Title = "SwipeItem IsEnabled Gallery";

			var swipeLayout = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var instructions = new Label
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
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
				BackgroundColor = Color.Red,
				IconImageSource = "calculator.png",
				Text = "Test 1"
			};

			swipeItem1.Invoked += (sender, e) => { DisplayAlert("SwipeView", "Test 1 Invoked", "Ok"); };

			var swipeItem2 = new SwipeItem
			{
				BackgroundColor = Color.Orange,
				IconImageSource = "coffee.png",
				Text = "Test 2"
			};

			swipeItem2.Invoked += (sender, e) => { DisplayAlert("SwipeView", "Test 2 Invoked", "Ok"); };

			var swipeItems = new SwipeItems { swipeItem1, swipeItem2 };

			swipeItems.SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.RemainOpen;
			swipeItems.Mode = SwipeMode.Reveal;

			var swipeContent = new Grid
			{
				BackgroundColor = Color.Gray
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
				Text = "Disable SwipeItem 1",
				WidthRequest = 250,
				VerticalOptions = LayoutOptions.Center
			};

			var swipeItem1Label = new Label
			{
				Text = "SwipeItem 1 is enabled",
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
				Text = "Disable SwipeItem 2",
				WidthRequest = 250,
				VerticalOptions = LayoutOptions.Center
			};

			var swipeItem2Label = new Label
			{
				Text = "SwipeItem 2 is enabled",
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
				swipeItem1.IsEnabled = !swipeItem1.IsEnabled;

				if (swipeItem1.IsEnabled)
				{
					swipeItem1Button.Text = "Disable SwipeItem 1";
					swipeItem1Label.Text = "SwipeItem 1 is enabled";
				}
				else
				{
					swipeItem1Button.Text = "Enable SwipeItem 1";
					swipeItem1Label.Text = "SwipeItem 1 is disabled";
				}
			};

			swipeItem2Button.Clicked += (sender, args) =>
			{
				swipeItem2.IsEnabled = !swipeItem2.IsEnabled;

				if (swipeItem2.IsEnabled)
				{
					swipeItem2Button.Text = "Disable SwipeItem 2";
					swipeItem2Label.Text = "SwipeItem 2 is enabled";
				}
				else
				{
					swipeItem2Button.Text = "Enable SwipeItem 2";
					swipeItem2Label.Text = "SwipeItem 2 is disabled";
				}
			};
		}
	}
}