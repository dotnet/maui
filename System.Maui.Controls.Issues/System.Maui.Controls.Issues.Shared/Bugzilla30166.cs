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
	[Issue(IssueTracker.Bugzilla, 30166, "NavigationBar.BarBackgroundColor resets on Lollipop after popping modal page", PlatformAffected.Android)]
	public class Bugzilla30166 : TestNavigationPage
	{
		protected override void Init()
		{
			BarBackgroundColor = Color.Red;

			Navigation.PushAsync(new ContentPage
			{
				Content = new Button
				{
					Text = "Push Modal",
					Command = new Command(async () => await Navigation.PushModalAsync(new ContentPage
					{
						Content = new Button
						{
							Text = "Back",
							Command = new Command(async () => await Navigation.PopModalAsync()),
						},
					})),
				},
			});
		}

#if UITEST
		[Test]
		public void Bugzilla30166Test()
		{
			RunningApp.WaitForElement(q => q.Marked("Push Modal"));
			RunningApp.Tap(q => q.Marked("Push Modal"));
			RunningApp.WaitForElement(q => q.Marked("Back"));
			RunningApp.Tap(q => q.Marked("Back"));
			RunningApp.Screenshot("Navigation bar should be red");
		}
#endif
	}
}