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
	[Issue(IssueTracker.Bugzilla, 43527, "[UWP] Detail title does not update when wrapped in a NavigationPage", PlatformAffected.WinRT)]
	public class Bugzilla43527 : TestMasterDetailPage
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

				Content = new StackLayout
				{
					Children = {
						new Label { Text = "Hello Page" },
						new Button { Text = "Change Title", Command = new Command(() => Title = $"New Title: {DateTime.Now.Second}") }
					}
				};
			}
		}
	}
}