using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 51553, "[UWP] Toolbar not shown on first Detail page", PlatformAffected.WinRT)]
	public class Bugzilla51553 : TestMasterDetailPage
	{
		protected override void Init()
		{
			Master = new ContentPage
			{
				Title = "Master",
				BackgroundColor = Color.Red
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