namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34563, "[iOS] Vertical SafeAreaEdge isn't respected when Top and Bottom constraints mismatch", PlatformAffected.iOS)]
public class Issue34563 : ContentPage
{
	public Issue34563()
	{
		SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.None, SafeAreaRegions.Container);

		BackgroundColor = Colors.Red;

		var topSafeBox = new ContentView
		{
			AutomationId = "TopSafeBox",
			BackgroundColor = Colors.LimeGreen,
			HeightRequest = 60,
			VerticalOptions = LayoutOptions.Start,
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container),
		};

		var bottomSafeBox = new ContentView
		{
			AutomationId = "BottomSafeBox",
			BackgroundColor = Colors.LimeGreen,
			HeightRequest = 60,
			VerticalOptions = LayoutOptions.End,
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container),
		};

		var instructions = new Label
		{
			AutomationId = "InstructionsLabel",
			Text = "TopSafeBox and BottomSafeBox (green) must never overlap the red background, which represents the OS safe areas (status bar / home indicator).",
			Margin = new Thickness(20),
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
			VerticalOptions = LayoutOptions.Center,
		};

		var grid = new Grid { AutomationId = "RootGrid" };
		grid.Add(instructions);
		grid.Add(topSafeBox);
		grid.Add(bottomSafeBox);

		Content = grid;
	}
}
