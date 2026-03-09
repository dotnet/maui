#if ANDROID
using Android.Views;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32041, "Shell with bottom tabs - Keyboard overlaps when SoftInput.AdjustResize is set", PlatformAffected.Android, issueTestNumber: 5)]
public class Issue32041Shell : Shell
{
	public Issue32041Shell()
	{
		AutomationId = "RootShell";
		Title = "Issue 32041 Shell";

		// Create TabBar for bottom tabs
		var tabBar = new TabBar();

		// Tab 1: Test page
		var testTab = new Tab
		{
			Title = "Test",
			Icon = "dotnet_bot.png"
		};

		var testPage = new ContentPage();

		// Full-height container to test keyboard resize behavior
		var mainContainer = new Grid
		{
			AutomationId = "MainContainer",
			BackgroundColor = Colors.LightCoral,
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
			Text = "Shell with Bottom Tabs + SoftInput.AdjustResize",
			FontSize = 18,
			FontAttributes = FontAttributes.Bold
		};

		var descriptionLabel = new Label
		{
			Text = "When keyboard appears, the entire Shell container (including bottom tabs) should resize. Tabs should move up and remain visible above keyboard.",
			FontSize = 14,
			TextColor = Colors.DarkRed
		};

		fillerContent.Add(topLabel);
		fillerContent.Add(descriptionLabel);

		// Bottom marker with entry
		var bottomMarker = new Border
		{
			AutomationId = "BottomMarker",
			BackgroundColor = Colors.DarkRed,
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
			Placeholder = "Tap here - tabs should move up",
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

		testPage.Content = mainContainer;

		var testShellContent = new ShellContent
		{
			Content = testPage
		};

		testTab.Items.Add(testShellContent);

		// Tab 2: Info page
		var infoTab = new Tab
		{
			Title = "Info",
			Icon = "dotnet_bot.png"
		};

		var infoPage = new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Spacing = 10,
				Children =
				{
					new Label
					{
						Text = "Tab 2",
						FontSize = 24,
						FontAttributes = FontAttributes.Bold
					},
					new Label
					{
						Text = "This tab is here to verify bottom tabs remain functional after keyboard interaction.",
						FontSize = 14
					}
				}
			}
		};

		var infoShellContent = new ShellContent
		{
			Content = infoPage
		};

		infoTab.Items.Add(infoShellContent);

		// Tab 3: Another tab for visual balance
		var moreTab = new Tab
		{
			Title = "More",
			Icon = "dotnet_bot.png"
		};

		var morePage = new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Spacing = 10,
				Children =
				{
					new Label
					{
						Text = "Tab 3",
						FontSize = 24,
						FontAttributes = FontAttributes.Bold
					},
					new Label
					{
						Text = "Another tab to test Shell bottom tab bar behavior with keyboard.",
						FontSize = 14
					}
				}
			}
		};

		var moreShellContent = new ShellContent
		{
			Content = morePage
		};

		moreTab.Items.Add(moreShellContent);

		// Add tabs to TabBar
		tabBar.Items.Add(testTab);
		tabBar.Items.Add(infoTab);
		tabBar.Items.Add(moreTab);

		// Add TabBar to Shell
		Items.Add(tabBar);

#if ANDROID
		// Set SoftInput.AdjustResize - the entire Shell container (including bottom tabs) should resize
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustResize | SoftInput.StateUnspecified);
#endif
	}
}
