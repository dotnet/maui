namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 16175, "OnNavigatedTo was not triggered when tabs in More section", PlatformAffected.iOS)]

public class Issue16175 : TabbedPage
{
	public Issue16175()
	{
		for (var i = 0; i < 10; i++)
		{
			Children.Add(new Issue16175Page(i.ToString()));
		}
	}
}

public class Issue16175Page : ContentPage
{
	private readonly Label _navigatedToLabel;

	public Issue16175Page(string title)
	{
		Title = title;
		_navigatedToLabel = new Label { Text = "NavigatedTo: Not triggered", AutomationId = "navigatedToLabel" };

		Content = new VerticalStackLayout
		{
			Children = { _navigatedToLabel }
		};
	}

	protected override void OnNavigatedTo(NavigatedToEventArgs args)
	{
		if (Parent is TabbedPage tabbed && tabbed.CurrentPage?.Title.Equals("8", StringComparison.Ordinal) == true)
		{
			_navigatedToLabel.Text = "NavigatedTo: Triggered";
		}
		base.OnNavigatedTo(args);
	}
}