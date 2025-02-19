using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 8366, "[Bug] UWP CollectionView Floating Row and Toolbar clipped")]
	public class Issue8366 : TestFlyoutPage
	{
		NavigationPage _items;
		NavigationPage _other;

		protected override void Init()
		{
			FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split;

			_items = new NavigationPage(Items());
			_other = new NavigationPage(Other());

			Detail = _items;
			Flyout = MasterPage();
		}

		ContentPage MasterPage()
		{
			var page = new ContentPage();

			var menu = new StackLayout();

			var instructions = new Label
			{
				Margin = 3,
				Text = "Tap 'Other' to change the Detail page. " +
				"Then tap 'Items' to return to this page. " +
				"If the CollectionView does not show a garbled mess at the top, this test has passed."
			};

			menu.Children.Add(instructions);

			var buttonItems = new Button { Text = "Items" };
			var buttonOther = new Button { Text = "Other" };

			page.Content = menu;

			buttonItems.Clicked += (sender, args) => { Detail = _items; };
			buttonOther.Clicked += (sender, args) => { Detail = _other; };

			menu.Children.Add(buttonItems);
			menu.Children.Add(buttonOther);

			page.Title = "8366 Flyout";

			return page;
		}

		ContentPage Items()
		{
			var page = new ContentPage
			{
				Title = "Items"
			};

			var cv = new CollectionView();

			var items = new List<string>() { "uno", "dos", "tres" };

			cv.ItemsSource = items;

			cv.ItemTemplate = new DataTemplate(() =>
			{
				var root = new Label();
				root.SetBinding(Label.TextProperty, new Binding("."));
				return root;
			});

			page.Content = cv;

			return page;
		}

		ContentPage Other()
		{
			var page = new ContentPage
			{
				Title = "Other",
				Content = new Label { Text = "Other page" }
			};

			return page;
		}
	}
}
