namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7181, "[Bug] Cannot update ToolbarItem text and icon", PlatformAffected.Android)]

public class Issue7181 : TestShell
{
	const string ToolbarBtn = "Toolbar button";
	const string DefaultToolbarItemText = "Toolbar test";
	const string AfterClickToolbarItemText = "Button Clicked";
	const string SetToolbarIconBtn = "Set toolbar icon button";

	int _clicks = 0;
	ToolbarItem _toolbarItem;

	protected override void Init()
	{
		var page = CreateContentPage("Test page");

		_toolbarItem = new ToolbarItem()
		{
			Text = DefaultToolbarItemText,
			AutomationId = ToolbarBtn,
			Command = new Command(OnToolbarClicked)
		};

		page.ToolbarItems.Add(_toolbarItem);
		page.Content = new StackLayout()
		{
			new Label
			{
				Text = "You should be able to change toolbar text"
			},
			new Button
			{
				AutomationId = SetToolbarIconBtn,
				Text = "Click to change toolbarIcon",
				Command = new Command(()=> _toolbarItem.IconImageSource = "coffee.png" )
			}
		};
	}

	private void OnToolbarClicked() =>
		_toolbarItem.Text = $"{AfterClickToolbarItemText} {_clicks++}";
}
