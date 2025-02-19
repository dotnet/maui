using System;
using System.Linq;

using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(UITestCategories.UwpIgnore)]
	[NUnit.Framework.Category(UITestCategories.Navigation)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 416, "NavigationPage in PushModal does not show NavigationBar", PlatformAffected.Android, NavigationBehavior.PushModalAsync)]
	public class Issue416 : TestNavigationPage
	{
		protected override void Init()
		{
			Navigation.PushAsync(new ContentPage
			{
				Title = "Test Page",
				Content = new Label
				{
					Text = "I should have a nav bar"
				}
			});
		}

		// Issue 416
		// NavigationBar should be visible in modal

#if UITEST
		[Test]
		[UiTest(typeof(NavigationPage))]
		public void Issue416TestsNavBarPresent()
		{
			RunningApp.WaitForElement(q => q.Marked("Test Page"));
			RunningApp.WaitForElement(q => q.Marked("I should have a nav bar"));
			RunningApp.Screenshot("All element present");
		}
#endif
	}
}
