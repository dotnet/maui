using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.SwipeView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8806, "Occasionally SwipeView rendering issues", PlatformAffected.iOS)]
	public class Issue8806 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Issue 8806";

			var layout = new StackLayout
			{
				Margin = new Thickness(12)
			};

			var instructions = new Label
			{
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "Swipe left and right several times and verify that the layout is always correct."
			};

			var leftSwipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				IconImageSource = "calculator.png",
				Text = "Delete"
			};

			leftSwipeItem.Invoked += (sender, e) => { DisplayAlert("SwipeView", "Delete Invoked", "Ok"); };

			var rightSwipeItem = new SwipeItem
			{
				BackgroundColor = Colors.LightGoldenrodYellow,
				IconImageSource = "calculator.png",
				Text = "Edit"
			};

			rightSwipeItem.Invoked += (sender, e) => { DisplayAlert("SwipeView", "Edit Invoked", "Ok"); };

			var leftSwipeItems = new SwipeItems { leftSwipeItem };
			leftSwipeItems.Mode = SwipeMode.Reveal;

			var rightSwipeItems = new SwipeItems { leftSwipeItem, rightSwipeItem };
			rightSwipeItems.Mode = SwipeMode.Reveal;

			var swipeContent = new Grid
			{
				BackgroundColor = Colors.Gray
			};

			var swipeLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Left or Right"
			};

			swipeContent.Children.Add(swipeLabel);

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				WidthRequest = 300,
				LeftItems = leftSwipeItems,
				RightItems = rightSwipeItems,
				Content = swipeContent
			};

			layout.Children.Add(instructions);
			layout.Children.Add(swipeView);

			Content = layout;
		}
	}
}