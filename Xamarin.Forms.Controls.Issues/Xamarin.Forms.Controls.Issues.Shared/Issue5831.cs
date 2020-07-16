using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Controls;
using System;
using System.Collections.Generic;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5831, "Navigating away from CollectionView and coming back leaves weird old items", PlatformAffected.iOS)]
	public class Issue5831 : TestShell
	{
		const string flyoutMainTitle = "Main";
		const string flyoutOtherTitle = "Other Page";

		protected override void Init()
		{
#if APP
			Device.SetFlags(new List<string>(Device.Flags ?? new List<string>()) { "CollectionView_Experimental" });

			Tab otherFlyoutItem = new Tab();
			Tab mainFlyoutItem = new Tab();

			string[] items = {
								"Baboon",
								"Capuchin Monkey",
								"Blue Monkey",
								"Squirrel Monkey",
								"Golden Lion Tamarin",
								"Howler Monkey",
								"Japanese Macaque",
							};
			var collectionView = new CollectionView() { VerticalOptions = LayoutOptions.FillAndExpand };
			collectionView.ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				return label;
			});
			collectionView.ItemsSource = items;
			var stackLayout = new StackLayout() { VerticalOptions = LayoutOptions.FillAndExpand };
			stackLayout.Children.Add(new Label { Text = "Go to the other page via the flyout, then come back. The items in the collection view should look identical when you return to this page." });
			stackLayout.Children.Add(collectionView);
			var collectionViewPage = new ContentPage { Content = stackLayout, BindingContext = this };
			mainFlyoutItem.Items.Add(collectionViewPage);

			otherFlyoutItem.Items.Add(new ContentPage { Content = new Label { Text = "Go back to main page via the flyout" } });

			Items.Add(new FlyoutItem
			{
				Title = flyoutMainTitle,
				Items = { mainFlyoutItem }
			});

			Items.Add(new FlyoutItem
			{
				Title = flyoutOtherTitle,
				Items = { otherFlyoutItem }
			});
#endif
		}

#if UITEST && __SHELL__
		[Test]
		public void CollectionViewRenderingWhenLeavingAndReturningViaFlyout()
		{
			TapInFlyout(flyoutOtherTitle);
			TapInFlyout(flyoutMainTitle);
		}
#endif
	}
}
