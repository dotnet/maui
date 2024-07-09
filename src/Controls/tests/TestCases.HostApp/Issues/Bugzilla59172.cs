using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla,
			59172, "[iOS] Popped page does not appear on top of current navigation stack, please file a bug.",
			PlatformAffected.iOS)]
	public class Bugzilla59172 : NavigationPage
	{
		public Bugzilla59172() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
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
						if (!safe)
						{ throw; }

						parent.navigationErrorLabel.Text = ex.Message;
					}
				}
			}
		}
	}
}