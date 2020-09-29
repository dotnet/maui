using System;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43469, "Calling DisplayAlert twice in WinRT causes a crash", PlatformAffected.WinRT)]

#if UITEST
	[NUnit.Framework.Category(UITestCategories.DisplayAlert)]
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Bugzilla)]
#endif
	public class Bugzilla43469 : TestContentPage
	{
		const string kButtonText = "Click to call DisplayAlert six times. Click as fast as you can to close them as they popup to ensure it doesn't crash.";
		protected override void Init()
		{
			var button = new Button { Text = kButtonText };

			button.Clicked += async (sender, args) =>
			{
				await DisplayAlert("First", "Text", "OK", "Cancel");
				await DisplayAlert("Second", "Text", "OK", "Cancel");
				await DisplayAlert("Three", "Text", "OK", "Cancel");
				Device.BeginInvokeOnMainThread(new Action(async () =>
				{
					await DisplayAlert("Fourth", "Text", "OK", "Cancel");
				}));

				Device.BeginInvokeOnMainThread(new Action(async () =>
				{
					await DisplayAlert("Fifth", "Text", "OK", "Cancel");
				}));

				Device.BeginInvokeOnMainThread(new Action(async () =>
				{
					await DisplayAlert("Sixth", "Text", "OK", "Cancel");
				}));
			};

			Content = button;
		}


#if UITEST

		[Test]
		public async Task Bugzilla43469Test()
		{
			RunningApp.WaitForElement(q => q.Marked(kButtonText));
			RunningApp.Tap(kButtonText);
			RunningApp.WaitForElement(q => q.Marked("First"));
			RunningApp.Tap(q => q.Marked("OK"));
			RunningApp.WaitForElement(q => q.Marked("Second"));
			RunningApp.Tap(q => q.Marked("OK"));
			RunningApp.WaitForElement(q => q.Marked("Three"));
			RunningApp.Tap(q => q.Marked("OK"));


			await Task.Delay(100);
			RunningApp.Tap(q => q.Marked("OK"));
			await Task.Delay(100);
			RunningApp.Tap(q => q.Marked("OK"));
			await Task.Delay(100);
			RunningApp.Tap(q => q.Marked("OK"));
			await Task.Delay(100);
			RunningApp.WaitForElement(q => q.Marked(kButtonText));
		}
#endif

	}
}