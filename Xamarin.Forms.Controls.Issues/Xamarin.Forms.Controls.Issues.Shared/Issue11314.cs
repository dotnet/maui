using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	[Issue(IssueTracker.Github, 11314,
		"Cannot swipe the swipeview inside a Listview[Bug]",
		PlatformAffected.Android)]
	public class Issue11314 : TestTabbedPage
	{
		public Issue11314()
		{
		}

		protected override void Init()
		{
			Title = "Issue 11314";

			var page1 = new Issue11314Page("Page 1");
			var page2 = new Issue11314Page("Page 2");

			Children.Add(page1);
			Children.Add(page2);
		}
	}

	public class Issue11314Page : ContentPage
	{
		public Issue11314Page(string title)
		{
			Title = title;

			for (int i = 0; i < 5; i++)
			{
				Items.Add(DateTime.Now.ToString());
			}

			var layout = new StackLayout
			{
				Padding = 0
			};

			var instructions = new Label
			{
				Padding = 12,
				BackgroundColor = Color.Black,
				TextColor = Color.White,
				Text = "Try to open the SwipeView doing swipe, if you can open it, the test has passed."
			};

			var listView = new ListView()
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				HasUnevenRows = true,
				ItemsSource = Items,
				ItemTemplate = new DataTemplate(() =>
				{
					var itemLayout = new StackLayout();

					var swipeView = new SwipeView();

					var content = new Grid
					{
						HeightRequest = 80,
						BackgroundColor = Color.LightGray
					};

					content.Children.Add(new Label
					{
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						Text = "Swipe to Left"
					});

					swipeView.Content = content;

					var swipeItem = new SwipeItem
					{
						BackgroundColor = Color.Red,
						Text = "Text"
					};

					swipeView.RightItems = new SwipeItems
					{
						swipeItem
					};

					itemLayout.Children.Add(swipeView);

					return new ViewCell { View = itemLayout };
				})
			};

			layout.Children.Add(instructions);
			layout.Children.Add(listView);

			Content = layout;
		}

		public ObservableCollection<string> Items = new ObservableCollection<string>();
	}
}