using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	// ScrollViewInitiallyEnabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
	// ScrollViewInitiallyEnabledThenDisabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
	// ScrollViewInitiallyNotEnabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
	// ScrollViewInitiallyNotEnabledThenEnabled (src\Compatibility\ControlGallery\src\Issues.Shared\ScrollViewIsEnabled.cs)
	[Issue(IssueTracker.None, 0112358, "ScrollView set to disabled will still allow scrolling", PlatformAffected.All)]
	public class ScrollViewIsEnabled : ContentPage
	{
		const string InitiallyEnabled = "Initially Enabled";
		const string InitiallyNotEnabled = "Initially Not Enabled";
		const string ToggleButton = "ToggleButton";
		const string ScrollView = "TheScrollView";
		const string FirstItem = "FirstItem";
		const string Success = "Success";

		public ScrollViewIsEnabled()
		{
			var initiallyEnabled = new Button { AutomationId = "InitiallyEnabled", Text = InitiallyEnabled };
			initiallyEnabled.Clicked += (sender, args) => { Navigation.PushAsync(ScrollViewTestPage(true)); };

			var initiallyNotEnabled = new Button { AutomationId= "InitiallyNotEnabled", Text = InitiallyNotEnabled };
			initiallyNotEnabled.Clicked += (sender, args) => { Navigation.PushAsync(ScrollViewTestPage(false)); };

			var layout = new StackLayout { Children = { initiallyNotEnabled, initiallyEnabled } };

			Content = layout;
		}

		static ContentPage ScrollViewTestPage(bool initiallyEnabled)
		{
			var scrollViewContents = new StackLayout();
			scrollViewContents.Children.Add(new Label { AutomationId = FirstItem, Text = FirstItem });

			for (int n = 0; n < 100; n++)
			{
				scrollViewContents.Children.Add(new Label { AutomationId = $"Item{n + 1}", Text = $"Item{n + 1}" });
			}

			var sv = new ScrollView { Content = scrollViewContents, IsEnabled = initiallyEnabled, AutomationId = ScrollView };
			var layout = new Grid { Margin = new Thickness(5, 40, 5, 0) };

			layout.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
			layout.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
			layout.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
			layout.RowDefinitions.Add(new RowDefinition());

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

			var success = new Label
			{
				AutomationId = Success
			};

			layout.Children.Add(instructions);
			layout.Children.Add(toggleButton);
			layout.Children.Add(success);
			layout.Children.Add(sv);

			Grid.SetRow(instructions, 0);
			Grid.SetRow(toggleButton, 1);
			Grid.SetRow(success, 2);
			Grid.SetRow(sv, 3);

			sv.Scrolled += (sender, args) => success.Text = Success;

			return new ContentPage { Content = layout };
		}
	}
}