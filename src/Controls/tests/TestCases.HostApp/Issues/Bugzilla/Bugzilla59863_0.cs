namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 59863, "TapGestureRecognizer extremely finicky", PlatformAffected.Android)]
public class Bugzilla59863_0 : TestContentPage
{
	int _singleTaps;
	const string SingleTapBoxId = "singleTapView";

	const string Singles = "singles(s)";

	protected override void Init()
	{
		var instructions = new Label
		{
			Text = "Tap the box below several times quickly. "
			+ "The number displayed below should match the number of times you tap the box."
		};

		var singleTapCounter = new Label { Text = $"{_singleTaps} {Singles}" };

		var singleTapBox = new BoxView
		{
			WidthRequest = 100,
			HeightRequest = 100,
			BackgroundColor = Colors.Bisque,
			AutomationId = SingleTapBoxId
		};

		var singleTap = new TapGestureRecognizer
		{
			Command = new Command(() =>
			{
				_singleTaps = _singleTaps + 1;
				singleTapCounter.Text = $"{_singleTaps} {Singles} on {SingleTapBoxId}";
			})
		};

		singleTapBox.GestureRecognizers.Add(singleTap);

		Content = new StackLayout
		{
			Margin = 40,
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill,
			Children = { instructions, singleTapBox, singleTapCounter }
		};
	}

}