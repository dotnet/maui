using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2837, " Exception thrown during NavigationPage.Navigation.PopAsync", PlatformAffected.Android)]
	public class Issue2837 : TestNavigationPage // or TestMasterDetailPage, etc ...
	{
		string _labelText = "worked";
		protected override async void Init()
		{
			// Initialize ui here instead of ctor
			await PushAsync(new ContentPage() { Title = "MainPage" });
		}

		protected override async void OnAppearing()
		{
			var nav = (NavigationPage)this;

			nav.Navigation.InsertPageBefore(new ContentPage() { Title = "SecondPage ", Content = new Label { Text = _labelText } }, nav.CurrentPage);
			await nav.Navigation.PopAsync(false);
			base.OnAppearing();
		}

#if UITEST
		[Test]
		public void Issue2837Test()
		{
			RunningApp.WaitForElement(q => q.Text (_labelText));
		}
#endif
	}
}