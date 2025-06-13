namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 16175, "OnNavigatedTo event triggered in More tabs", PlatformAffected.iOS)]

public class Issue16175 : TabbedPage
{
	public Issue16175()
	{
		for (var i = 0; i < 10; i++)
		{
			var page = new Issue16175Page { Title = $"Tab{i + 1}" };
			Children.Add(page);
		}
	}
}

public class Issue16175Page : ContentPage
{
	Label _navigatedToLabel;

	public Issue16175Page()
	{
		_navigatedToLabel = new Label { Text = "NavigatedTo: Not triggered", AutomationId = "navigatedToLabel" };

		Content = new VerticalStackLayout
		{
			Children = { _navigatedToLabel }
		};
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		if (Title is not null && Title == "Tab8" && _navigatedToLabel is not null)
		{
			_navigatedToLabel.Text = "NavigatedTo: Triggered";
		}
		base.OnNavigatedTo(args);
	}
}