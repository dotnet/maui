namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 1776, "Button Released not being triggered", PlatformAffected.macOS)]
public class GitHub1776 : TestContentPage
{
	Label PressedLabel;
	int _pressedCount;
	int PressedCount
	{
		get { return _pressedCount; }
		set
		{
			_pressedCount = value;
			PressedLabel.Text = $"Pressed: {_pressedCount}";
		}
	}

	Label ReleasedLabel;
	int _releasedCount;
	int ReleasedCount
	{
		get { return _releasedCount; }
		set
		{
			_releasedCount = value;
			ReleasedLabel.Text = $"Released: {_releasedCount}";
		}
	}

	Label ClickedLabel;
	int _clickedCount;
	int ClickedCount
	{
		get { return _clickedCount; }
		set
		{
			_clickedCount = value;
			ClickedLabel.Text = $"Clicked: {_clickedCount}";
		}
	}

	Label CommandLabel;
	int _commandCount;
	int CommandCount
	{
		get { return _commandCount; }
		set
		{
			_commandCount = value;
			CommandLabel.Text = $"Command: {_commandCount}";
		}
	}

	protected override void Init()
	{
		PressedLabel = new Label { AutomationId = "PressedLabel" };
		ReleasedLabel = new Label { AutomationId = "ReleasedLabel" };
		ClickedLabel = new Label { AutomationId = "ClickedLabel" };
		CommandLabel = new Label { AutomationId = "CommandLabel" };

		var button = new Button
		{
			Text = "Press me!",
			AutomationId = "TheButton"
		};
		button.Pressed += (s, e) =>
		{
			PressedCount++;
		};
		button.Released += (s, e) =>
		{
			ReleasedCount++;
		};
		button.Clicked += (s, e) =>
		{
			ClickedCount++;
		};
		button.Command = new Command(() =>
		{
			CommandCount++;
		});

		PressedCount = 0;
		ReleasedCount = 0;
		ClickedCount = 0;
		CommandCount = 0;

		StackLayout layout = new StackLayout();

		layout.Children.Add(button);
		layout.Children.Add(PressedLabel);
		layout.Children.Add(ReleasedLabel);
		layout.Children.Add(ClickedLabel);
		layout.Children.Add(CommandLabel);

		Content = layout;
	}
}
