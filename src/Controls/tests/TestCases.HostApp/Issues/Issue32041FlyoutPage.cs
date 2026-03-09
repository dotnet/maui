#if ANDROID
using Android.Views;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32041, "FlyoutPage - Keyboard overlaps when SoftInput.AdjustResize is set", PlatformAffected.Android, issueTestNumber: 4)]
public class Issue32041FlyoutPage : FlyoutPage
{
	public Issue32041FlyoutPage()
	{
		AutomationId = "RootFlyoutPage";
		Title = "Issue 32041 FlyoutPage";

		// Create Flyout (menu) Page
		var flyoutPage = new ContentPage
		{
			Title = "Menu",
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Spacing = 10,
				Children =
				{
					new Label
					{
						Text = "Flyout Menu",
						FontSize = 24,
						FontAttributes = FontAttributes.Bold
					},
					new Label
					{
						Text = "This is the flyout/menu side. The detail page tests keyboard resize.",
						FontSize = 14
					},
					new Button
					{
						Text = "Toggle Flyout",
						AutomationId = "ToggleFlyoutButton",
						Command = new Command(() => IsPresented = !IsPresented)
					}
				}
			}
		};

		// Create Detail Page with test content
		var detailPage = new ContentPage
		{
			Title = "Detail"
		};

		// Full-height container to test keyboard resize behavior
		var mainContainer = new Grid
		{
			AutomationId = "MainContainer",
			BackgroundColor = Colors.LightGreen,
			RowDefinitions =
			{
				new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
				new RowDefinition { Height = GridLength.Auto }
			}
		};

		var fillerContent = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 10
		};

		var topLabel = new Label
		{
			AutomationId = "TopLabel",
			Text = "FlyoutPage with SoftInput.AdjustResize",
			FontSize = 18,
			FontAttributes = FontAttributes.Bold
		};

		var descriptionLabel = new Label
		{
			Text = "When keyboard appears, the detail page container should resize. This ensures the flyout and detail both remain above the keyboard.",
			FontSize = 14,
			TextColor = Colors.DarkGreen
		};

		var openFlyoutButton = new Button
		{
			Text = "Open Flyout Menu",
			AutomationId = "OpenFlyoutButton",
			Margin = new Thickness(0, 20, 0, 0),
			Command = new Command(() => IsPresented = true)
		};

		fillerContent.Add(topLabel);
		fillerContent.Add(descriptionLabel);
		fillerContent.Add(openFlyoutButton);

		// Bottom marker with entry
		var bottomMarker = new Border
		{
			AutomationId = "BottomMarker",
			BackgroundColor = Colors.DarkGreen,
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

		var testEntry = new Entry
		{
			Placeholder = "Tap here - detail should resize",
			AutomationId = "TestEntry",
			BackgroundColor = Colors.White,
			WidthRequest = 250
		};

		markerContent.Add(markerLabel);
		markerContent.Add(testEntry);
		bottomMarker.Content = markerContent;

		mainContainer.Add(fillerContent);
		mainContainer.Add(bottomMarker);
		Grid.SetRow(fillerContent, 0);
		Grid.SetRow(bottomMarker, 1);

		detailPage.Content = mainContainer;

		// Assign Flyout and Detail
		Flyout = flyoutPage;
		Detail = new NavigationPage(detailPage);

#if ANDROID
		// Set SoftInput.AdjustResize - the entire FlyoutPage container should resize
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustResize | SoftInput.StateUnspecified);
#endif
	}
}
