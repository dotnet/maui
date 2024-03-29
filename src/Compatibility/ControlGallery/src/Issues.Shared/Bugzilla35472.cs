using System;
using System.Diagnostics;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using NUnit.Framework;
using Xamarin.UITest;

#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 35472, "PopAsync during ScrollToAsync throws NullReferenceException")]
	public class Bugzilla35472 : TestNavigationPage
	{
		protected override void Init()
		{
			// Set up the scroll viewer page
			var scrollToButton = new Button() { Text = "Now push this button" };

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

			PushAsync(startPage);

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

#if UITEST
		[Test]
		[UiTest(typeof(NavigationPage))]
		public void Issue35472PopAsyncDuringAnimatedScrollToAsync()
		{
			RunningApp.WaitForElement(q => q.Marked("Push this button"));
			RunningApp.Tap(q => q.Marked("Push this button"));

			RunningApp.WaitForElement(q => q.Marked("Now push this button"));
			RunningApp.Screenshot("On Page With ScrollView");
			RunningApp.Tap(q => q.Marked("Now push this button"));

			RunningApp.WaitForElement("The test has passed");
			RunningApp.Screenshot("Success");
		}
#endif
	}
}