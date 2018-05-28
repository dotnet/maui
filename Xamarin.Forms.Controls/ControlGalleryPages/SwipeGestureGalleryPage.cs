using System;

namespace Xamarin.Forms.Controls
{
	public class SwipeGestureGalleryPage : ContentPage
	{
		public class SwipeContainer : ContentView
		{
			public EventHandler SwipeLeft;
			public EventHandler SwipeRight;
			public EventHandler SwipeUp;
			public EventHandler SwipeDown;

			public SwipeContainer()
			{
				GestureRecognizers.Add(GetSwipeLeft());
				GestureRecognizers.Add(GetSwipeRight());
				GestureRecognizers.Add(GetSwipeUp());
				GestureRecognizers.Add(GetSwipeDown());
			}

			SwipeGestureRecognizer GetSwipeLeft()
			{
				var swipe = new SwipeGestureRecognizer();
				swipe.Direction = SwipeDirection.Left;
				swipe.Swiped += (sender, args) => SwipeLeft?.Invoke(this, new EventArgs());
				return swipe;
			}

			SwipeGestureRecognizer GetSwipeRight()
			{
				var swipe = new SwipeGestureRecognizer();
				swipe.Direction = SwipeDirection.Right;
				swipe.Swiped += (sender, args) => SwipeRight?.Invoke(this, new EventArgs());
				return swipe;
			}

			SwipeGestureRecognizer GetSwipeUp()
			{
				var swipe = new SwipeGestureRecognizer();
				swipe.Direction = SwipeDirection.Up;
				swipe.Swiped += (sender, args) => SwipeUp?.Invoke(this, new EventArgs());
				return swipe;
			}

			SwipeGestureRecognizer GetSwipeDown()
			{
				var swipe = new SwipeGestureRecognizer();
				swipe.Direction = SwipeDirection.Down;
				swipe.Swiped += (sender, args) => SwipeDown?.Invoke(this, new EventArgs());
				return swipe;
			}
		}

		public SwipeGestureGalleryPage()
		{
			var box = new Image
			{
				BackgroundColor = Color.Gray,
				WidthRequest = 500,
				HeightRequest = 500,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};

			var label = new Label { Text = "Use one finger and swipe inside the gray box." };

			var swipeme = new SwipeContainer { Content = box };
			swipeme.SwipeLeft += (sender, args) => label.Text = "You swiped left.";
			swipeme.SwipeRight += (sender, args) => label.Text = "You swiped right.";
			swipeme.SwipeUp += (sender, args) => label.Text = "You swiped up.";
			swipeme.SwipeDown += (sender, args) => label.Text = "You swiped down.";

			Content = new StackLayout { Children = { label, swipeme }, Padding = new Thickness(20) };
		}
	}
}