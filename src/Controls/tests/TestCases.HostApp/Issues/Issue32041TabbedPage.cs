using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32041, "Keyboard with TabbedPage bottom tabs - verify tabs remain visible", PlatformAffected.Android, issueTestNumber: 3)]
public class Issue32041TabbedPage : Microsoft.Maui.Controls.TabbedPage
{
	public Issue32041TabbedPage()
	{
		AutomationId = "RootTabbedPage";
		Title = "Issue 32041 TabbedPage";
		BarBackgroundColor = Colors.DarkBlue;
		BarTextColor = Colors.White;
		SelectedTabColor = Colors.Yellow;
		UnselectedTabColor = Colors.LightGray;

		// Set toolbar placement to bottom for Android
		On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);

		// Add tabs to TabbedPage
		Children.Add(CreateContentPage("Tab 1"));
		Children.Add(CreateContentPage("Tab 2"));
#if ANDROID
		// Set SoftInput.AdjustResize to simulate the user's scenario
		// This should cause the content to be resized (not overlapped) when the keyboard appears
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(Android.Views.SoftInput.AdjustResize | Android.Views.SoftInput.StateUnspecified);
#endif

	}

	ContentPage CreateContentPage(string title)
	{
		// Full-height container with distinct visual marker at bottom
		// We'll measure the bottom marker's position to verify content moves up when keyboard appears
		var mainContainer = new Grid
		{
			AutomationId = "MainContainer",
			BackgroundColor = Colors.LightBlue,
			RowDefinitions =
			{
				new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
				new RowDefinition { Height = GridLength.Auto }
			}
		};

		// Filler content to ensure container fills screen
		var fillerContent = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 10
		};

		var topLabel = new Label
		{
			AutomationId = "TopLabel",
			Text = "Test keyboard overlap with SoftInput.AdjustResize",
			FontSize = 18,
			FontAttributes = FontAttributes.Bold
		};

		var descriptionLabel = new Label
		{
			Text = "With AdjustResize, bottom padding is applied when keyboard appears. The bottom marker should move up to stay visible above the keyboard.",
			FontSize = 14,
			TextColor = Colors.DarkSlateGray
		};

		fillerContent.Add(topLabel);
		fillerContent.Add(descriptionLabel);

		// Bottom marker - this should move up to stay visible above keyboard
		// We test by measuring the marker's Y position before and after keyboard appears
		var bottomMarker = new Border
		{
			AutomationId = "BottomMarker",
			BackgroundColor = Colors.Red,
			HeightRequest = 60,
			Margin = new Thickness(0)
		};

		var markerContent = new VerticalStackLayout
		{
			Spacing = 5,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		var markerLabel = new Label
		{
			Text = "BOTTOM MARKER",
			FontSize = 12,
			FontAttributes = FontAttributes.Bold,
			TextColor = Colors.White,
			HorizontalOptions = LayoutOptions.Center
		};

		var testEntry = new Microsoft.Maui.Controls.Entry
		{
			Placeholder = "Tap here to show keyboard",
			AutomationId = "TestEntry",
			BackgroundColor = Colors.White,
			WidthRequest = 250
		};

		markerContent.Add(markerLabel);
		markerContent.Add(testEntry);
		bottomMarker.Content = markerContent;

		mainContainer.Add(fillerContent);
		Grid.SetRow(fillerContent, 0);

		mainContainer.Add(bottomMarker);
		Grid.SetRow(bottomMarker, 1);

		return new ContentPage
		{
			Content = mainContainer,
			Title = title
		};
	}
}
