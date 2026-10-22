namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36816, "Android: Clicks being passed through to controls under other Views", PlatformAffected.Android)]
public class Issue36816 : ContentPage
{
	public Issue36816()
	{
		var parentTapCount = 0;
		var parentResult = new Label { Text = "0", AutomationId = "ParentTapCount" };
		var child = new Label
		{
			Text = "Tap anywhere on this card",
			AutomationId = "CardLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
		var card = new ContentView
		{
			BackgroundColor = Colors.Green,
			HeightRequest = 100,
			AutomationId = "CardView",
			Content = child
		};
		var grid = new Grid { AutomationId = "ParentGrid", Children = { card } };
		var parentTap = new TapGestureRecognizer();
		parentTap.Tapped += (sender, e) => parentResult.Text = (++parentTapCount).ToString();
		grid.GestureRecognizers.Add(parentTap);

		var toggleTransparency = new Button
		{
			Text = "Set InputTransparent to true",
			AutomationId = "ToggleTransparency"
		};
		toggleTransparency.Clicked += (sender, e) =>
		{
			card.InputTransparent = true;
			toggleTransparency.Text = "InputTransparent is true";
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 12,
			Children =
			{
				grid,
				toggleTransparency,
				new Label { Text = "Parent taps:" },
				parentResult
			}
		};
	}
}
