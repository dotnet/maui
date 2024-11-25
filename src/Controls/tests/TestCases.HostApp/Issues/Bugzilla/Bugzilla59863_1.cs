namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 59863, "TapGestureRecognizer extremely finicky1", PlatformAffected.Android,
	issueTestNumber: 1)]
public class Bugzilla59863_1 : TestContentPage
{
	int _doubleTaps;
	const string DoubleTapBoxId = "doubleTapView";

	const string Doubles = "double(s)";

	protected override void Init()
	{
		var instructions = new Label
		{
			Text = "Tap the box below once. The counter should not increment. "
					+ "Double tap the box. The counter should increment."
		};

		var doubleTapCounter = new Label { Text = $"{_doubleTaps} {Doubles} on {DoubleTapBoxId}" };

		var doubleTapBox = new BoxView
		{
			WidthRequest = 100,
			HeightRequest = 100,
			BackgroundColor = Colors.Chocolate,
			AutomationId = DoubleTapBoxId
		};

		var doubleTap = new TapGestureRecognizer
		{
			NumberOfTapsRequired = 2,
			Command = new Command(() =>
			{
				_doubleTaps = _doubleTaps + 1;
				doubleTapCounter.Text = $"{_doubleTaps} {Doubles} on {DoubleTapBoxId}";
			})
		};

		doubleTapBox.GestureRecognizers.Add(doubleTap);

		Content = new StackLayout
		{
			Margin = 40,
			HorizontalOptions = LayoutOptions.Fill,
			VerticalOptions = LayoutOptions.Fill,
			Children = { instructions, doubleTapBox, doubleTapCounter }
		};
	}
}