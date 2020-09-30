using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
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
	[Issue(IssueTracker.Github, 11251,
		"[Bug] SwipeView on UWP Executes Command Twice",
		PlatformAffected.UWP)]
	public class Issue11251 : TestContentPage
	{
		public Issue11251()
		{
		}

		public ICommand ExecuteMe = new Command(() =>
		{
			Debug.WriteLine("Executing Command...");
		});

		protected override void Init()
		{
			Title = "Issue 11251";

			BindingContext = this;

			var layout = new StackLayout();

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Swipe to the right and tap the SwipeItem. Verify that the command has only been executed once."
			};

			var swipeView = new SwipeView();

			var swipeContent = new Grid
			{
				BackgroundColor = Color.LightGray,
				HeightRequest = 80
			};

			var info = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe To Right"
			};

			swipeContent.Children.Add(info);

			swipeView.Content = swipeContent;

			var swipeItem = new SwipeItem
			{
				BackgroundColor = Color.Red,
				Text = "Execute Command",
				Command = ExecuteMe
			};

			swipeView.LeftItems = new SwipeItems
			{
				swipeItem
			};

			layout.Children.Add(instructions);
			layout.Children.Add(swipeView);

			Content = layout;

			swipeItem.Invoked += (sender, args) =>
			{
				Debug.WriteLine("SwipeItem Invoked...");
			};
		}
	}
}