using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.SwipeView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11286,
		"[Bug] SwipeItem clicked event not working",
		PlatformAffected.Android)]
	public class Issue11286 : TestContentPage
	{
		public Issue11286()
		{
		}

		protected override void Init()
		{
			Title = "Issue 11286";

			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Colors.Black,
				TextColor = Colors.White,
				Text = "Swipe to the right and tap the SwipeItem. If the text below the SwipeView is updated, the test has passed."
			};

			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Colors.Red,
				Text = "Text"
			};

			swipeView.LeftItems = new SwipeItems
			{
				swipeItem
			};

			var swipeContent = new Grid
			{
				HeightRequest = 80,
				BackgroundColor = Colors.LightGray
			};

			var label = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to Right"
			};

			swipeContent.Children.Add(label);

			swipeView.Content = swipeContent;

			var result = new Label();

			layout.Children.Add(instructions);
			layout.Children.Add(swipeView);
			layout.Children.Add(result);

			Content = layout;

			swipeItem.Clicked += (sender, args) =>
			{
				result.TextColor = Colors.Green;
				result.Text = "The test has passed";
			};
		}
	}
}