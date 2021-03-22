using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43527, "[UWP] Detail title does not update when wrapped in a NavigationPage", PlatformAffected.WinRT)]
	public class Bugzilla43527 : TestFlyoutPage
	{
		protected override void Init()
		{
			Flyout = new ContentPage
			{
				Title = "Flyout",
				BackgroundColor = Color.Red
			};

			Detail = new NavigationPage(new TestPage());
		}

		class TestPage : ContentPage
		{
			public TestPage()
			{
				Title = "Test Page";

				Content = new StackLayout
				{
					Children = {
						new Label { Text = "Hello Page" },
						new Button { Text = "Change Title", Command = new Command(() => Title = $"New Title: {DateTime.Now.Second}") }
					}
				};
			}
		}

#if UITEST
		[Test]
		public void TestB43527UpdateTitle()
		{
			RunningApp.WaitForElement(q => q.Marked("Change Title"));
			RunningApp.WaitForElement(q => q.Marked("Test Page"));
			RunningApp.Tap(q => q.Marked("Change Title"));
			RunningApp.WaitForNoElement(q => q.Marked("Test Page"));
		}
#endif
	}
}