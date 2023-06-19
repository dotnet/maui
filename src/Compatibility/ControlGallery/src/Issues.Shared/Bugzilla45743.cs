using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 45743, "[iOS] Calling DisplayAlert via BeginInvokeOnMainThread blocking other calls on iOS", PlatformAffected.iOS)]
	public class Bugzilla45743 : TestNavigationPage
	{
		protected override void Init()
		{
			PushAsync(new ContentPage
			{
				Content = new StackLayout
				{
					AutomationId = "Page1",
					Children =
					{
						new Label { Text = "Page 1" }
					}
				}
			});

			Device.BeginInvokeOnMainThread(async () =>
			{
				await DisplayAlert("Title", "Message", "Accept", "Cancel");
			});

			Device.BeginInvokeOnMainThread(async () =>
			{
				await PushAsync(new ContentPage
				{
					AutomationId = "Page2",
					Content = new StackLayout
					{
						Children =
						{
							new Label { Text = "Page 2" }
						}
					}
				});
			});

			Device.BeginInvokeOnMainThread(async () =>
			{
				await DisplayAlert("Title 2", "Message", "Accept", "Cancel");
			});

			Device.BeginInvokeOnMainThread(async () =>
			{
				await DisplayActionSheet("ActionSheet Title", "Cancel", "Close", new string[] { "Test", "Test 2" });
			});
		}

#if UITEST

#if __IOS__
		[Test]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Bugzilla45743Test()
		{
			RunningApp.WaitForElement(q => q.Marked("ActionSheet Title"));
			RunningApp.Tap("Close");
			RunningApp.WaitForElement(q => q.Marked("Title 2"));
			RunningApp.Tap("Accept");
			RunningApp.WaitForElement(q => q.Marked("Title"));
			RunningApp.Tap("Accept");
			Assert.True(RunningApp.Query(q => q.Text("Page 2")).Length > 0);
		}
#endif

#endif
	}
}