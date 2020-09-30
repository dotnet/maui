using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10679, "SwipeView crash with NullReferenceException",
		PlatformAffected.Android)]
	public partial class Issue10679 : TestShell
	{
		public Issue10679()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}

	[Preserve(AllMembers = true)]
	public class FirstIssue10679Page : ContentPage
	{
		public FirstIssue10679Page()
		{
			Title = "Issue 10679";

			var layout = new Grid();

			var button = new Button
			{
				VerticalOptions = LayoutOptions.Center,
				Text = "Go to Next Page"
			};

			layout.Children.Add(button);

			Content = layout;

			button.Clicked += async (sender, args) =>
			{
				await Shell.Current.Navigation.PushAsync(new SecondIssue10679Page()).ConfigureAwait(false);
			};
		}
	}

	[Preserve(AllMembers = true)]
	public class SecondIssue10679Page : ContentPage
	{
		public SecondIssue10679Page()
		{
			var layout = new Grid();

			var swipeView = new SwipeView();

			var leftItem = new SwipeItem
			{
				BackgroundColor = Color.Red
			};

			var leftItems = new SwipeItems
			{
				Mode = SwipeMode.Execute,
				SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close
			};

			leftItems.Add(leftItem);

			var rightItem = new SwipeItem
			{
				BackgroundColor = Color.Green
			};

			var rightItems = new SwipeItems
			{
				Mode = SwipeMode.Execute,
				SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close
			};

			rightItems.Add(rightItem);

			var content = new Grid
			{
				BackgroundColor = Color.White
			};

			var contentLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Swipe Left or Right"
			};

			content.Children.Add(contentLabel);

			swipeView.Content = content;
			swipeView.LeftItems = leftItems;
			swipeView.RightItems = rightItems;

			layout.Children.Add(swipeView);

			Content = layout;

			leftItem.Invoked += async (sender, args) =>
			{
				Shell.Current.Navigation.InsertPageBefore(new ThirdIssue10679Page(), this);
				_ = await Shell.Current.Navigation.PopAsync(false);
			};

			rightItem.Invoked += async (sender, args) =>
			{
				Shell.Current.Navigation.InsertPageBefore(new ThirdIssue10679Page(), this);
				_ = await Shell.Current.Navigation.PopAsync(false);
			};
		}
	}

	[Preserve(AllMembers = true)]
	public class ThirdIssue10679Page : ContentPage
	{
		public ThirdIssue10679Page()
		{
			var layout = new StackLayout();

			var infoLabel = new Label
			{
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				Text = "Without NullReferenceException, the test has passed."
			};

			layout.Children.Add(infoLabel);

			Content = layout;
		}
	}
}