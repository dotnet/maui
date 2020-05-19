using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers=true)]
	[Issue (IssueTracker.Github, 194, "iOS tab edit has no done button to return", PlatformAffected.iOS)]
	public class Issue194 : TabbedPage
	{
		public Issue194 ()
		{
			Title = "Issue 194";

			var leavePageBtn = new Button {
				Text = "Leave"
			};

			// May have unexpected behavior but navigation page is needed to replicate the bug.
			leavePageBtn.Clicked += (s, e) => Navigation.PopModalAsync ();

			var pageOne = new ContentPage {
				Title = "Page 1",
				Content = leavePageBtn
			};
			var pageTwo = new ContentPage {
				Title = "Page 2"
			};
			var pageThree = new ContentPage {
				Title = "Page 3"
			};
			var pageFour = new ContentPage {
				Title = "Page 4"
			};
			var pageFive = new ContentPage {
				Title = "Page 5"
			};
			var pageSix = new ContentPage {
				Title = "Page 6"
			};
			var pageSeven = new ContentPage {
				Title = "Page 7"
			};
			var pageEight = new ContentPage {
				Title = "Page 8"
			};
			var pageNine = new ContentPage {
				Title = "Page 9"
			};

			if (Device.RuntimePlatform == Device.iOS) {
				// Create an overflow amount of tabs depending on device
				if (Device.Idiom == TargetIdiom.Tablet) {
					Children.Add (pageOne);
					Children.Add (pageTwo);
					Children.Add (pageThree);
					Children.Add (pageFour);
					Children.Add (pageFive);
					Children.Add (pageSix);
					Children.Add (pageSeven);
					Children.Add (pageEight);
					Children.Add (pageNine);
				} else {
					Children.Add (pageOne);
					Children.Add (pageTwo);
					Children.Add (pageThree);
					Children.Add (pageFour);
					Children.Add (pageFive);
					Children.Add (pageSix);
				}
			}
		}
	}
}
