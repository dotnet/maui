using System.Collections.Generic;
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
				BackgroundColor = Colors.Red
			};

			var leftItems = new SwipeItems
			{
				Mode = SwipeMode.Execute,
				SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close
			};

			leftItems.Add(leftItem);

			var rightItem = new SwipeItem
			{
				BackgroundColor = Colors.Green
			};

			var rightItems = new SwipeItems
			{
				Mode = SwipeMode.Execute,
				SwipeBehaviorOnInvoked = SwipeBehaviorOnInvoked.Close
			};

			rightItems.Add(rightItem);

			var content = new Grid
			{
				BackgroundColor = Colors.White
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