using System;
using System.Linq;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 764, "Keyboard does not dismiss on SearchBar", PlatformAffected.Android)]
	public class Issue764 : TestContentPage
	{

		protected override void Init ()
		{
			Title = "Issue 764";

			var searchBar = new SearchBar {
				Placeholder = "Search Me!"
			};

			var label = new Label {
				Text = "Pending Search"
			};

			searchBar.SearchButtonPressed += (s, e) => label.Text = "Search Activated";

			var layout = new StackLayout { 
				Children =  {
					searchBar,
					label
				}
			};

			Content = layout;
		}

		// Issue 416
		// NavigationBar should be visible in modal

#if UITEST
		[Test]
		[Category(UITestCategories.ManualReview)]
		public void Issue764TestsKeyboardDismissedForEnter ()
		{
			Assert.Inconclusive ("Needs test");
		}
#endif

    }
}
