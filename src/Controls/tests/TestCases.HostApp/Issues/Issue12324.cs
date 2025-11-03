#if TEST_FAILS_ON_CATALYST // TabBar not visible on Catalyst: https://github.com/dotnet/maui/issues/32329

using AndroidSpecific = Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 12324, "Tabbedpage should not have visual bug", PlatformAffected.Android)]

public class Issue12324 : TabbedPage
{
	public Issue12324()
	{
		// Set tabs to bottom placement on Android
#if Android
		AndroidSpecific.TabbedPage.SetToolbarPlacement(this, AndroidSpecific.ToolbarPlacement.Bottom);
#endif
		BarBackground = new RadialGradientBrush
		{
			GradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Color.FromArgb("#1AFFFF00"), Offset = 0.0f },
				new GradientStop { Color = Colors.Green, Offset = 1.0f }
			}
		};

		Children.Add(new ContentPage
		{
			IconImageSource = "groceries.png",
			Title = "Tab 1",
			Content = new VerticalStackLayout
			{
				Children = {
					new Label { AutomationId = "Label12324", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, Text = "Welcome to .NET MAUI!"
					}
				}
			}
		});
	}
}
#endif