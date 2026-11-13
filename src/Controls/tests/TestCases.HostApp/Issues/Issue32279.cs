namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32279, "TapGestureRecognizer does not work on layouts without a Background on Windows", PlatformAffected.UWP)]
public class Issue32279 : TestContentPage
{
	const string TapAnchorNoBackground = "TapAnchorNoBackground";
	const string TapAnchorWithBackground = "TapAnchorWithBackground";
	const string ResultLabelNoBackground = "ResultLabelNoBackground";
	const string ResultLabelWithBackground = "ResultLabelWithBackground";

	protected override void Init()
	{
		var layout = new StackLayout
		{
			Spacing = 20,
			Padding = new Thickness(20)
		};

		// ContentView with NO background
		var resultLabelNoBackground = new Label
		{
			AutomationId = ResultLabelNoBackground,
			Text = "Waiting",
			FontSize = 18,
		};

		var contentViewNoBackground = new ContentView
		{
			HeightRequest = 200,
			WidthRequest = 300,
			Content = new Label
			{
				Text = "Tap below me (no background)",
				AutomationId = TapAnchorNoBackground,
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Fill,
				HorizontalTextAlignment = TextAlignment.Center,
			}
		};

		contentViewNoBackground.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(() => resultLabelNoBackground.Text = "Tapped"),
		});

		// ContentView WITH explicit background
		var resultLabelWithBackground = new Label
		{
			AutomationId = ResultLabelWithBackground,
			Text = "Waiting",
			FontSize = 18,
		};

		var contentViewWithBackground = new ContentView
		{
			HeightRequest = 200,
			WidthRequest = 300,
			BackgroundColor = Colors.LightGray,
			Content = new Label
			{
				Text = "Tap below me (with background)",
				AutomationId = TapAnchorWithBackground,
				VerticalOptions = LayoutOptions.Start,
				HorizontalOptions = LayoutOptions.Fill,
				HorizontalTextAlignment = TextAlignment.Center,
			}
		};

		contentViewWithBackground.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(() => resultLabelWithBackground.Text = "Tapped"),
		});

		layout.Children.Add(contentViewNoBackground);
		layout.Children.Add(resultLabelNoBackground);
		layout.Children.Add(contentViewWithBackground);
		layout.Children.Add(resultLabelWithBackground);

		Content = layout;
	}
}