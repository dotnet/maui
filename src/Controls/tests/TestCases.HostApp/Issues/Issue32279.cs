namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32279, "TapGestureRecognizer does not work on layouts without a Background on Windows", PlatformAffected.UWP)]
public class Issue32279 : TestContentPage
{
	const string TapTargetNoBackground = "TapTargetNoBackground";
	const string TapTargetWithBackground = "TapTargetWithBackground";
	const string ResultLabelNoBackground = "ResultLabelNoBackground";
	const string ResultLabelWithBackground = "ResultLabelWithBackground";

	protected override void Init()
	{
		var layout = new StackLayout { Spacing = 20, Padding = new Thickness(20) };

		// Grid with NO background
		var resultLabelNoBackground = new Label
		{
			AutomationId = ResultLabelNoBackground,
			Text = "Waiting",
			FontSize = 18,
		};

		var gridNoBackground = new Grid
		{
			HeightRequest = 100,
			WidthRequest = 300,
		};

		// 👉 Tap target REAL (Appium sí lo puede tocar)
		var tapLabelNoBackground = new Label
		{
			Text = "Tap me (no background)",
			AutomationId = TapTargetNoBackground,
			VerticalOptions = LayoutOptions.Fill,
			HorizontalOptions = LayoutOptions.Fill,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
		};

		gridNoBackground.Children.Add(tapLabelNoBackground);

		gridNoBackground.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(() => resultLabelNoBackground.Text = "Tapped"),
		});

		// Grid WITH explicit background
		var resultLabelWithBackground = new Label
		{
			AutomationId = ResultLabelWithBackground,
			Text = "Waiting",
			FontSize = 18,
		};

		var gridWithBackground = new Grid
		{
			HeightRequest = 100,
			WidthRequest = 300,
			BackgroundColor = Colors.LightGray,
		};

		// 👉 Tap target REAL
		var tapLabelWithBackground = new Label
		{
			Text = "Tap me (with background)",
			AutomationId = TapTargetWithBackground,
			VerticalOptions = LayoutOptions.Fill,
			HorizontalOptions = LayoutOptions.Fill,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
		};

		gridWithBackground.Children.Add(tapLabelWithBackground);

		gridWithBackground.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(() => resultLabelWithBackground.Text = "Tapped"),
		});

		layout.Children.Add(gridNoBackground);
		layout.Children.Add(resultLabelNoBackground);
		layout.Children.Add(gridWithBackground);
		layout.Children.Add(resultLabelWithBackground);

		Content = layout;
	}
}