using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using System.ComponentModel;
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.SwipeView)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8753, "Assign custom SwipeItems in the SwipeView", PlatformAffected.All)]
	public class Issue8753 : TestContentPage
	{
		public Issue8753()
		{
			Title = "Issue 8753";

			var layout = new StackLayout();

			var instructions = new Label
			{
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "If you can see and open the SwipeView below, the test has passed."
			};

			var deleteSwipeItem = new SwipeItem { BackgroundColor = Color.Red, Text = "Delete", IconImageSource = "coffee.png" };

			deleteSwipeItem.Invoked += (sender, e) =>
			{
				DisplayAlert("SwipeView", "Delete Invoked", "OK");
			};

			var swipeView = new SwipeView
			{
				HeightRequest = 60,
				BackgroundColor = Color.LightGray,
				LeftItems = new SwipeItems(new List<SwipeItem> { deleteSwipeItem })
				{
					Mode = SwipeMode.Reveal
				},
				RightItems = new SwipeItems
				{
					deleteSwipeItem
				}
			};

			var content = new Grid
			{
				BackgroundColor = Color.LightGoldenrodYellow
			};

			var info = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe to the Right"
			};

			content.Children.Add(info);

			swipeView.Content = content;

			layout.Children.Add(instructions);
			layout.Children.Add(swipeView);

			Content = layout;
		}

		protected override void Init()
		{

		}
	}
}
