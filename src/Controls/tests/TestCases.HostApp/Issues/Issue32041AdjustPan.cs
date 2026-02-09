namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32041, "Verify AdjustPan mode does not apply keyboard insets", PlatformAffected.Android, issueTestNumber: 2)]
public class Issue32041AdjustPan : ContentPage
{
	public Issue32041AdjustPan()
	{
		AutomationId = "RootPagePan";
		Title = "Issue 32041 AdjustPan";

		// Full-height container to test AdjustPan behavior
		// With AdjustPan, container height should NOT change (window pans instead)
		var mainContainerPan = new Grid
		{
			AutomationId = "MainContainerPan",
			BackgroundColor = Colors.LightYellow,
			RowDefinitions =
			{
				new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
				new RowDefinition { Height = GridLength.Auto }
			}
		};

		var topContent = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 10
		};

		var topLabelPan = new Label
		{
			AutomationId = "TopLabelPan",
			Text = "Test keyboard with SoftInput.AdjustPan",
			FontSize = 18,
			FontAttributes = FontAttributes.Bold
		};

		var descriptionLabel = new Label
		{
			Text = "With AdjustPan, the container height should NOT change when keyboard appears. The window pans (moves) instead of resizing.",
			FontSize = 14,
			TextColor = Colors.DarkGreen
		};

		topContent.Add(topLabelPan);
		topContent.Add(descriptionLabel);

		// Bottom marker
		var bottomMarkerPan = new Border
		{
			AutomationId = "BottomMarkerPan",
			BackgroundColor = Colors.Green,
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
			Text = "BOTTOM MARKER (PAN)",
			FontSize = 12,
			FontAttributes = FontAttributes.Bold,
			TextColor = Colors.White,
			HorizontalOptions = LayoutOptions.Center
		};

		var testEntryPan = new Entry
		{
			Placeholder = "Tap here to show keyboard",
			AutomationId = "TestEntryPan",
			BackgroundColor = Colors.White,
			WidthRequest = 250
		};

		markerContent.Add(markerLabel);
		markerContent.Add(testEntryPan);
		bottomMarkerPan.Content = markerContent;

		mainContainerPan.Add(topContent);
		Grid.SetRow(topContent, 0);

		mainContainerPan.Add(bottomMarkerPan);
		Grid.SetRow(bottomMarkerPan, 1);

		Content = mainContainerPan;

#if ANDROID
		// Set SoftInput.AdjustPan - this should NOT apply insets
		// The window should pan instead
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(Android.Views.SoftInput.AdjustPan | Android.Views.SoftInput.StateUnspecified);
#endif
	}
}
