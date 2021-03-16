namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.SwipeViewGalleries
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
				HorizontalOptions = LayoutOptions.Start,
				Text = "Open SwipeView"
			};

			var closeButton = new Button
			{
				HorizontalOptions = LayoutOptions.Start,
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
				BackgroundColor = Color.Red,
				IconImageSource = "calculator.png",
				Text = "File"
			};

			swipeItem.Invoked += (sender, e) => { DisplayAlert("SwipeView", "File Invoked", "Ok"); };

			var swipeItems = new SwipeItems { swipeItem };

			swipeItems.Mode = SwipeMode.Reveal;

			var swipeContent = new Grid
			{
				BackgroundColor = Color.Gray
			};

			var fileSwipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right (File)"
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

			Content = swipeLayout;

			openButton.Clicked += (sender, e) =>
			{
				bool animated = animatedCheckBox.IsChecked;
				swipeView.Open(OpenSwipeItem.LeftItems, animated);
			};

			closeButton.Clicked += (sender, e) =>
			{
				bool animated = animatedCheckBox.IsChecked;
				swipeView.Close(animated);
			};
		}
	}
}