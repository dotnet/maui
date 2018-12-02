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
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 
			59172, "[iOS] Popped page does not appear on top of current navigation stack, please file a bug.", 
			PlatformAffected.iOS)]
	public class Bugzilla59172 : TestNavigationPage
	{
		protected override void Init()
		{
			var firstPage = new TestPage();
			Navigation.PushAsync(firstPage);
		}

		[Preserve(AllMembers = true)]
		public class TestPage : ContentPage
		{
			TestPage parent;
			Label navigationErrorLabel = new Label();

			public TestPage(TestPage parentPage = null)
			{
				this.parent = parentPage;

				var layout = new StackLayout();

				var forwardButton = new Button { Text = "Forward", AutomationId = "GoForward" };
				layout.Children.Add(forwardButton);
				forwardButton.Clicked += Forward_OnClicked;

				if (parentPage != null)
				{
					var backButton = new Button { Text = "Back (Delayed)", AutomationId = "GoBackDelayed" };
					layout.Children.Add(backButton);
					backButton.Clicked += (a, b) => BackButtonPress(false);

					var backButtonSafe = new Button { Text = "Back (Delayed; Safe)", AutomationId = "GoBackDelayedSafe" };
					layout.Children.Add(backButtonSafe);
					backButtonSafe.Clicked += (a, b) => BackButtonPress(true);
				}

				layout.Children.Add(navigationErrorLabel);

				Content = layout;
			}

			void Forward_OnClicked(object sender, EventArgs e)
			{
				Navigation.PushAsync(new TestPage(this));
			}

			async void BackButtonPress(bool safe)
			{
				try
				{
					// Assume some workload that delays the back navigation
					await Task.Delay(2500);

					await Navigation.PopAsync();
				}
				catch (Exception ex)
				{
					if (!safe) { throw; }

					parent.navigationErrorLabel.Text = ex.Message;
				}
			}
		}

#if UITEST

		// Test scenario: Tapping the GoBack link triggers a PopAsync 2500ms after the tap event.
		//   Right before PopAsync is triggered, manually navigate back pressing the back arrow in the navigation bar

		[Test]
		public async void Issue59172Test()
		{
			RunningApp.Tap(q => q.Marked("GoForward"));
			RunningApp.Tap(q => q.Marked("GoBackDelayed"));
			RunningApp.Back();

			await Task.Delay(1000);

			// App should not have crashed
			RunningApp.WaitForElement(q => q.Marked("GoForward"));
		}

		[Test]
		public async void Issue59172RecoveryTest()
		{
			RunningApp.Tap(q => q.Marked("GoForward"));
			RunningApp.Tap(q => q.Marked("GoBackDelayedSafe"));
			RunningApp.Back();

			await Task.Delay(1000);

			RunningApp.Tap(q => q.Marked("GoForward"));

			// App should navigate
			RunningApp.WaitForElement(q => q.Marked("GoBackDelayedSafe"));
		}
#endif
	}
}