using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

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
	[Issue(IssueTracker.Bugzilla, 51553, "[UWP] Toolbar not shown on first Detail page", PlatformAffected.WinRT)]
	public class Bugzilla51553 : TestFlyoutPage
	{
		protected override void Init()
		{
			Flyout = new ContentPage
			{
				Title = "Flyout",
				BackgroundColor = Colors.Red
			};

			Detail = new NavigationPage(new TestPage());
		}

		class TestPage : ContentPage
		{
			public TestPage()
			{
				Title = "Test Page";

				ToolbarItems.Add(new ToolbarItem("Test", "coffee.png", () => System.Diagnostics.Debug.WriteLine("ToolbarItem pressed")));

				Content = new StackLayout
				{
					Children = {
						new Label { Text = "If the ToolbarItem is not visible then this test has failed." }
					}
				};
			}
		}
	}
}