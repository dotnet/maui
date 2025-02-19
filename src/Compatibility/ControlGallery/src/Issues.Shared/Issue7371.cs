using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7371, "iOS race condition(or not checking for null) of refreshing(offset animation) causes NullReferenceException", PlatformAffected.iOS)]
	public class Issue7371 : TestContentPage
	{
		ListView ListView => Content as ListView;
		protected override void Init()
		{
			Content = new ListView();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			ListView.IsRefreshing = true;

			Application.Current.MainPage = new ContentPage() { Content = new Label { Text = "Success", VerticalOptions = LayoutOptions.Center } };

			ListView.IsRefreshing = false;
		}

#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
		[Test]
		public async Task RefreshingListViewCrashesWhenDisposedTest()
		{
			await Task.Delay(500);
			RunningApp.WaitForElement(q => q.Marked("Success"));
		}
#endif
	}
}