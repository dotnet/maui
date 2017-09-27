using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.LifeCycle)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 11, "Config changes which restart the app should not crash", 
		PlatformAffected.Android)]
	public class RestartAppTest : TestContentPage 
	{
		public const string ForceRestart = "ForceRestart";
		public const string Success = "Success";
		public const string RestartButton = "Restart";

		protected override void Init()
		{
			var restartButton = new Button { Text = "Restart", AutomationId = RestartButton };
			restartButton.Clicked += (sender, e) => MessagingCenter.Send(this, ForceRestart);

			Content = new StackLayout
			{
				Padding = new Thickness(0, 20, 0, 0),
				Children =
				{
					new Label { Text = Success },
					restartButton
				}
			};
		}

#if UITEST
		[Test]
		public void ForcingRestartDoesNotCauseCrash()
		{
			RunningApp.WaitForElement(RestartButton);
			RunningApp.Tap(RestartButton);

			// If the app hasn't crashed, this test has passed
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}