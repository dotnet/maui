using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
#endif
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.None, 0,"TabbedPage nav tests", PlatformAffected.All)]
	public class TabbedPageTests : TestTabbedPage
	{
		protected override void Init ()
		{
			var popButton1 = new Button () { Text = "Pop", BackgroundColor = Color.Blue };
			popButton1.Clicked += (s, a) => Navigation.PopModalAsync ();

			var popButton2 = new Button () { Text = "Pop 2", BackgroundColor = Color.Blue };
			popButton2.Clicked += (s, a) => Navigation.PopModalAsync ();

			Children.Add (new ContentPage () { Title = "Page 1", Content = popButton1 });
			Children.Add (new ContentPage () { Title = "Page 2", Content = popButton2 });
		}

#if UITEST
		[Test]
		public void TabbedPageWithModalIssueTestsAllElementsPresent ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Page 1"));
			RunningApp.WaitForElement (q => q.Marked ("Page 2"));
			RunningApp.WaitForElement (q => q.Button ("Pop"));

			RunningApp.Screenshot ("All elements present");
		}

		[Test]
		public void TabbedPageWithModalIssueTestsPopFromFirstTab ()
		{
			RunningApp.Tap (q => q.Button ("Pop"));
			RunningApp.WaitForElement (q => q.Marked ("Bug Repro's"));

			RunningApp.Screenshot ("Popped from first tab");
		}

		[Test]
		public void TabbedPageWithModalIssueTestsPopFromSecondTab ()
		{
			RunningApp.Tap (q => q.Marked ("Page 2"));
			RunningApp.WaitForElement (q => q.Button ("Pop 2"));
			RunningApp.Screenshot ("On second tab");

			RunningApp.Tap (q => q.Button ("Pop 2"));
			RunningApp.WaitForElement (q => q.Marked ("Bug Repro's"));
			RunningApp.Screenshot ("Popped from second tab");
		}
#endif
	}
}
