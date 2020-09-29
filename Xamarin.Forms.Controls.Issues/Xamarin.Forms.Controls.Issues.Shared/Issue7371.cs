using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
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
		[Test]
		public async Task RefreshingListViewCrashesWhenDisposedTest()
		{
			await Task.Delay(500);
			RunningApp.WaitForElement(q => q.Marked("Success"));
		}
#endif
	}
}