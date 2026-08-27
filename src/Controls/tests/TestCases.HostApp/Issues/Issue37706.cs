[Issue(
	IssueTracker.Github,
	37706,
	"Page.OnBackButtonPressed is not invoked on a root page",
	PlatformAffected.Android)]
public class Issue37706 : ContentPage
{
	int _backPressCount;
	readonly Label _statusLabel;

	public Issue37706()
	{
		_statusLabel = new Label
		{
			AutomationId = "BackButtonPressedStatus",
			Text = "OnBackButtonPressed not called",
		};

		Content = new VerticalStackLayout
		{
			Children =
			{
				new Label
				{
					AutomationId = "RootPageLabel",
					Text = "Root page",
				},
				_statusLabel,
			},
		};
	}

	protected override bool OnBackButtonPressed()
	{
		_backPressCount++;
		_statusLabel.Text = $"OnBackButtonPressed called {_backPressCount} time";
		return true;
	}
}
