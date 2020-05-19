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
	[Issue(IssueTracker.Bugzilla, 51802, "[UWP] Detail Page Has Navigation Bar Even When Not Inside a NavigationPage", PlatformAffected.WinRT)]
	public class Bugzilla51802 : TestMasterDetailPage
	{
		protected override void Init()
		{
			Master = new ContentPage
			{
				Title = "Master",
				BackgroundColor = Color.Red
			};

			Detail = new ContentPage
			{
				Content = new StackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					Children = {
						new Label { Text = "If a navigation bar is present on this page then this test has failed." }
					}
				}
			};
		}
	}
}
