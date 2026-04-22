namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31177, "ScrollView.ScrollToAsync doesn't work when called from Page.OnAppearing", PlatformAffected.All)]
public class Issue31177 : ContentPage
{
	readonly ScrollView _scrollView;
	readonly Label _statusLabel;

	public Issue31177()
	{
		_statusLabel = new Label
		{
			Text = "Waiting",
			AutomationId = "StatusLabel"
		};

		// Tall content that forces scrolling; the success label is below the fold
		var tallSpacer = new BoxView
		{
			HeightRequest = 2000,
			Color = Colors.LightGray
		};

		var successLabel = new Label
		{
			Text = "ScrollToAsync Succeeded",
			AutomationId = "SuccessLabel"
		};

		_scrollView = new ScrollView
		{
			AutomationId = "TheScrollView",
			Content = new VerticalStackLayout
			{
				Children = { tallSpacer, successLabel }
			}
		};

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				_statusLabel,
				_scrollView
			}
		};

		Grid.SetRow(_statusLabel, 0);
		Grid.SetRow(_scrollView, 1);
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		// This is the scenario from the issue: ScrollToAsync called from OnAppearing
		// On iOS, this would fail because ContentSize was empty and the pending scroll was never executed.
		await _scrollView.ScrollToAsync(0, 2000, false);
		_statusLabel.Text = "Scrolled";
	}
}
