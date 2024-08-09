using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 35472, "PopAsync during ScrollToAsync throws NullReferenceException")]
	public class Bugzilla35472 : NavigationPage
	{
		public Bugzilla35472() : base(new MainPage())
		{
		}

		public class MainPage : ContentPage
		{
			public MainPage()
			{
				// Set up the scroll viewer page
				var scrollToButton = new Button() { AutomationId = "NowPushButton", Text = "Now push this button" };

				var stackLayout = new StackLayout();

				stackLayout.Children.Add(scrollToButton);

				for (int n = 0; n < 100; n++)
				{
					stackLayout.Children.Add(new Label() { Text = n.ToString() });
				}

				var scrollView = new ScrollView()
				{
					Content = stackLayout
				};

				var pageWithScrollView = new ContentPage()
				{
					Content = scrollView
				};

				// Set up the start page
				var goButton = new Button()
				{
					AutomationId = "PushButton",
					Text = "Push this button"
				};

				var successLabel = new Label() { Text = "The test has passed", IsVisible = false };

				var startPage = new ContentPage()
				{
					Content = new StackLayout
					{
						VerticalOptions = LayoutOptions.Center,
						Children = {
						goButton,
						successLabel
					}
					}
				};

				Navigation.PushAsync(startPage);

				goButton.Clicked += (sender, args) => Navigation.PushAsync(pageWithScrollView);

				scrollToButton.Clicked += async (sender, args) =>
				{
					try
					{
#pragma warning disable 4014
						// Deliberately not awaited so we can simulate a user navigating back before the scroll is finished
						scrollView.ScrollToAsync(0, 1500, true);
#pragma warning restore 4014
						await Navigation.PopAsync();
						successLabel.IsVisible = true;
					}
					catch (Exception ex)
					{
						Debug.WriteLine(ex);
					}
				};
			}
		}
	}
}