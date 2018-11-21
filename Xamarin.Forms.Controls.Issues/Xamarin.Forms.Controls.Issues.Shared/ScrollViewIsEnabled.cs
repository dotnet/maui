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
	[Category(UITestCategories.IsEnabled)]
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0112358, "ScrollView set to disabled will still allow scrolling", PlatformAffected.All)]
	public class ScrollViewIsEnabled : TestNavigationPage
	{
		const string InitiallyEnabled = "Initially Enabled";
		const string InitiallyNotEnabled = "Initially Not Enabled";
		const string ToggleButton = "ToggleButton";
		const string ScrollView = "TheScrollView";
		const string FirstItem = "FirstItem";
		const string Success = "Success";

		protected override void Init()
		{
			var initiallyEnabled = new Button { Text = InitiallyEnabled };
			initiallyEnabled.Clicked += (sender, args) => { Navigation.PushAsync(ScrollViewTestPage(true)); };

			var initiallyNotEnabled = new Button { Text = InitiallyNotEnabled };
			initiallyNotEnabled.Clicked += (sender, args) => { Navigation.PushAsync(ScrollViewTestPage(false)); };

			var layout = new StackLayout { Children = { initiallyNotEnabled, initiallyEnabled } };

			var root = new ContentPage { Content = layout };

			PushAsync(root);
		}

		static ContentPage ScrollViewTestPage(bool initiallyEnabled)
		{
			var scrollViewContents = new StackLayout();
			scrollViewContents.Children.Add(new Label { Text = FirstItem });
			for (int n = 0; n < 100; n++)
			{
				scrollViewContents.Children.Add(new Label { Text = n.ToString() });
			}

			var sv = new ScrollView { Content = scrollViewContents, IsEnabled = initiallyEnabled, AutomationId = ScrollView };
			var layout = new StackLayout { Margin = new Thickness(5, 40, 5, 0) };

			var toggleButton = new Button { Text = $"Toggle IsEnabled (currently {sv.IsEnabled})", AutomationId = ToggleButton };

			toggleButton.Clicked += (sender, args) =>
			{
				sv.IsEnabled = !sv.IsEnabled;
				toggleButton.Text = $"Toggle IsEnabled (currently {sv.IsEnabled})";
			};

			var instructions = new Label
			{
				Text = @"Attempt to scroll the ScrollView below. 
If 'IsEnabled' is false and the ScrollView scrolls, this test has failed. 
If 'IsEnabled' is true and the ScrollView does not scroll, this test has failed. 
Use the toggle button to check both values of 'IsEnabled'."
			};

			var success = new Label();

			layout.Children.Add(instructions);
			layout.Children.Add(toggleButton);
			layout.Children.Add(success);
			layout.Children.Add(sv);

			sv.Scrolled += (sender, args) => success.Text = Success;

			return new ContentPage { Content = layout };
		}

#if UITEST
		[Test]
		public void ScrollViewInitiallyEnabled()
		{
			RunningApp.WaitForElement(InitiallyEnabled);
			RunningApp.Tap(InitiallyEnabled);
			RunningApp.WaitForElement(FirstItem);
			RunningApp.WaitForElement(ScrollView);
			RunningApp.ScrollDown(ScrollView, ScrollStrategy.Gesture);
			RunningApp.WaitForElement(Success); // If the ScrollView scrolled, the success label should be displayed
		}

		[Test]
		public void ScrollViewInitiallyEnabledThenDisabled()
		{
			RunningApp.WaitForElement(InitiallyEnabled);
			RunningApp.Tap(InitiallyEnabled);
			RunningApp.WaitForElement(ToggleButton);
			RunningApp.Tap(ToggleButton);
			
			// Scrolling should now be IsEnabled = false

			RunningApp.WaitForElement(FirstItem);
			RunningApp.WaitForElement(ScrollView);
			RunningApp.ScrollDown(ScrollView, ScrollStrategy.Gesture);
			RunningApp.WaitForNoElement(Success); // Shouldn't have scrolled, so no success label should be displayed
		}

		[Test]
		public void ScrollViewInitiallyNotEnabled()
		{
			RunningApp.WaitForElement(InitiallyNotEnabled);
			RunningApp.Tap(InitiallyNotEnabled);
			RunningApp.WaitForElement(FirstItem);
			RunningApp.WaitForElement(ScrollView);
			RunningApp.ScrollDown(ScrollView, ScrollStrategy.Gesture);
			RunningApp.WaitForNoElement(Success); // Shouldn't have scrolled, so no success label should be displayed
		}

		[Test]
		public void ScrollViewInitiallyNotEnabledThenEnabled()
		{
			RunningApp.WaitForElement(InitiallyNotEnabled);
			RunningApp.Tap(InitiallyNotEnabled);
			RunningApp.WaitForElement(ToggleButton);
			RunningApp.Tap(ToggleButton);

			// Scrolling should now be IsEnabled = true

			RunningApp.WaitForElement(FirstItem);
			RunningApp.WaitForElement(ScrollView);
			RunningApp.ScrollDown(ScrollView, ScrollStrategy.Gesture);
			RunningApp.WaitForElement(Success); // If the ScrollView scrolled, the success label should be displayed
		}
#endif
	}
}