using System;
using System.Linq;
using Maui.Controls.Sample.Pages.Base;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public class SwipeGestureGalleryPage : BasePage
	{
		public class SwipeContainer : VerticalStackLayout
		{
			public EventHandler? SwipeLeft;
			public EventHandler? SwipeRight;
			public EventHandler? SwipeUp;
			public EventHandler? SwipeDown;

			public SwipeContainer()
			{
			}


			public View? Content
			{
				get => (View)(Children.LastOrDefault()!);
				set
				{
					if (Children.Count > 0)
						Remove(Children[0]);

					Add(value);
					value!.GestureRecognizers.Add(GetSwipeRight());
					value!.GestureRecognizers.Add(GetSwipeLeft());
					value!.GestureRecognizers.Add(GetSwipeUp());
					value!.GestureRecognizers.Add(GetSwipeDown());
				}
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
				BackgroundColor = Colors.Gray,
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