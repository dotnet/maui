namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23801, "Span GestureRecognizers don't work when the span is wrapped over two lines", PlatformAffected.Android)]
public partial class Issue23801 : ContentPage
{
	Label testLabel;
	StackLayout stackLayout;
	Span span;
	Label label;
	FormattedString formattedString;
	TapGestureRecognizer tapGestureRecognizer;

	public Issue23801()
	{
		stackLayout = new StackLayout();
		label = new Label();
		testLabel = new Label();
		formattedString = new FormattedString();
		tapGestureRecognizer = new TapGestureRecognizer();

		label.AutomationId = "Label";
		formattedString.Spans.Add(new Span { Text = "I am the start of a span " });
		tapGestureRecognizer.Tapped += OnTapGestureRecognizerTapped;
		stackLayout.VerticalOptions = LayoutOptions.Center;

		if (DeviceInfo.Platform == DevicePlatform.MacCatalyst || DeviceInfo.Platform == DevicePlatform.WinUI)
		{
			span = new Span
			{
				Text = "https://en.wikipedia.org/wiki/Maui-Lable-Control-Span-Gesture-Test-This-Is-A-Long-Link-That-Will-Wrap-Over-Two-Lines-On-Windows-And-MacCatalyst-And-It-Should-Be-Tappable-At-The-End-Of-The-First-Line-And-Should-Display-A-Message"
			};

		}
		else if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS)
		{
			span = new Span
			{
				Text = "https://en.wikipedia.org/wiki/Maui-Lable-Control-Span-Gesture-Test",
			};
		}

		span.TextDecorations = TextDecorations.Underline;
		span.TextColor = Colors.Blue;
		span.GestureRecognizers.Add(tapGestureRecognizer);
		formattedString.Spans.Add(span);
		label.FormattedText = formattedString;
		stackLayout.Children.Add(label);

		testLabel.Text = "Label Span not tapped";
		testLabel.AutomationId = "TestLabel";
		testLabel.Margin = new Thickness(0, 20, 0, 0);
		stackLayout.Children.Add(testLabel);

		Content = stackLayout;
	}

	private void OnTapGestureRecognizerTapped(object sender, TappedEventArgs e)
	{
		testLabel.Text = "Label span tapped";
	}
}