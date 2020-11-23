using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System.Collections.Generic;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.CollectionView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11090,
		"[Bug] UWP: PopAsync causes a crash when called from a CollectionView.SelectionChanged event",
		PlatformAffected.UWP)]
	public class Issue11090 : TestContentPage
	{
		protected override void Init()
		{
			Title = "Issue 11090";

			var layout = new StackLayout();

			var navButton = new Button
			{
				Text = "Navigate"
			};

			layout.Children.Add(navButton);

			Content = layout;

			navButton.Clicked += (sender, args) =>
			{
				Navigation.PushAsync(new Issue111090Page2());
			};
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue111090Page2 : ContentPage
	{
		public Issue111090Page2()
		{
			Title = "Issue 11090";

			var layout = new StackLayout();

			var collectionView = new CollectionView
			{
				SelectionMode = SelectionMode.Single,
				ItemsSource = new List<string>()
				{
					"Item 1",
					"Item 2",
					"Item 3"
				}
			};

			layout.Children.Add(collectionView);

			Content = layout;

			collectionView.SelectionChanged += async (sender, args) =>
			{
				await Navigation.PopAsync();
			};
		}
	}
}
