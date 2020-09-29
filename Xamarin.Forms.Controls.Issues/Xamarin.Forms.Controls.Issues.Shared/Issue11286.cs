using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
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
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Swipe to the right and tap the SwipeItem. If the text below the SwipeView is updated, the test has passed."
			};

			var swipeView = new SwipeView();

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Color.Red,
				Text = "Text"
			};

			swipeView.LeftItems = new SwipeItems
			{
				swipeItem
			};

			var swipeContent = new Grid
			{
				HeightRequest = 80,
				BackgroundColor = Color.LightGray
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
				result.TextColor = Color.Green;
				result.Text = "The test has passed";
			};
		}
	}
}