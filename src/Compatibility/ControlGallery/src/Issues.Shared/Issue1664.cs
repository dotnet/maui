using System;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1664, "Page.Title bubbling", PlatformAffected.iOS)]
	public class Issue1664 : TabbedPage
	{
		public Issue1664()
		{
			NavigationPage nav1 = new NavigationPage(new ContentPage { Title = "Page1" });
			NavigationPage nav2 = new NavigationPage(new PageTwo());

			nav1.Title = "Tab 1";
			nav2.Title = "Tab 2";

			Children.Add(nav1);
			Children.Add(nav2);
		}

		public class PageTwo : ContentPage
		{
			public PageTwo()
			{
				var pageTwoEntry = new Entry
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.FillAndExpand,
					Placeholder = "Enter a title for page 2",
					Text = "Page 2",
				};

				pageTwoEntry.SetBinding(Entry.TextProperty, new Binding("Title", BindingMode.OneWayToSource));
				BindingContext = this;
				Content = new StackLayout
				{
					Children = {
						new Label {Text = "When changing the content of this entry, the page title (on top) should update, but not the tab title"},
						pageTwoEntry
					}
				};
			}
		}
	}
}

