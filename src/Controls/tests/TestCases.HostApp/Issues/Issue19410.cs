namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19410, "[iOS] Canvas.DrawImage and Canvas.Rotate cut Image", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue19410 : TestContentPage
{
	BoxView _boxView;

	protected override void Init()
	{
		BuildUI();
	}

	void OnRotateTo60Clicked(object sender, EventArgs args)
	{
		_boxView?.Rotation = 60;
	}

	void BuildUI()
	{
		ScrollView scrollView = new ScrollView();
		VerticalStackLayout stackLayout = new VerticalStackLayout();

		Button button = new Button
		{
			Text = "Rotate to 60",
			AutomationId = "RotateBtn"
		};
		button.Clicked += OnRotateTo60Clicked;

		stackLayout.Children.Add(button);

		Grid grid = new Grid
		{
			Margin = new Thickness(10, 50)
		};

		grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(500) });

		Border border = new Border
		{
			Stroke = Colors.Red,
			StrokeThickness = 1
		};

		_boxView = new BoxView
		{
			Color = Colors.Red,
			HeightRequest = 300,
			WidthRequest = 500
		};

		border.Content = _boxView;
		grid.Children.Add(border);

		stackLayout.Children.Add(grid);

		scrollView.Content = stackLayout;
		this.Content = scrollView;
	}
}
