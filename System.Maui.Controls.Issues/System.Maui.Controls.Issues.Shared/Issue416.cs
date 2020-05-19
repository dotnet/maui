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
#if UITEST
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
	[NUnit.Framework.Category(UITestCategories.Navigation)]
#endif
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Github, 416, "NavigationPage in PushModal does not show NavigationBar", PlatformAffected.Android, NavigationBehavior.PushModalAsync)]
	public class Issue416 : TestNavigationPage
	{
		protected override void Init ()
		{
			Navigation.PushAsync (new ContentPage {
				Title = "Test Page",
				Content = new Label {
					Text = "I should have a nav bar"
				}
			});
		}

		// Issue 416
		// NavigationBar should be visible in modal

#if UITEST
		[Test]
		[UiTest (typeof(NavigationPage))]
		public void Issue416TestsNavBarPresent ()
		{
			RunningApp.WaitForElement (q => q.Marked ("Test Page"));
			RunningApp.WaitForElement (q => q.Marked ("I should have a nav bar"));
			RunningApp.Screenshot ("All element present");
		}
#endif
    }
}
