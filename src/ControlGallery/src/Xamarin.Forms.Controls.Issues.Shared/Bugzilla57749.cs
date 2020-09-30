using System;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 57749, "After enabling a disabled button it is not clickable", PlatformAffected.UWP)]
	public class Bugzilla57749 : TestContentPage
	{
		protected override void Init()
		{
			button1.Text = "Click me";
			button1.AutomationId = "btnClick";
			button1.IsEnabled = false;
			button1.Clicked += Button1_Clicked1;
			this.Content = button1;
		}
		Button button1 = new Button();

		private void Button1_Clicked1(object sender, EventArgs e)
		{
			this.DisplayAlert("Button test", "Button was clicked", "Ok");
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await Task.Delay(100);
			button1.IsEnabled = true;
		}

#if UITEST
		[Test]
		public async Task Bugzilla57749Test()
		{
			await Task.Delay(500);
			RunningApp.Tap(c => c.Marked("btnClick"));
			RunningApp.WaitForElement (q => q.Marked ("Button was clicked"));
			RunningApp.Tap("Ok");
		}
#endif
	}
}