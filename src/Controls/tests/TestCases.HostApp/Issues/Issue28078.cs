namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28078, "Crash occurs when click on flyout menu", PlatformAffected.Android)]
public class Issue28078 : Shell
{
	public Issue28078()
	{
		Items.Add(new _28078MainPage(this));
	}

	public void LoggedIn()
	{
		Items.Clear();
		Items.Add(new _28708Page1());
		Items.Add(new _28708Page2());
	}
}

public class _28078MainPage : ContentPage
{
	public _28078MainPage(Issue28078 shell)
	{
		Title = "Main Page";

		var button = new Button
		{
			Text = "Click",
			AutomationId = "button"
		};
		button.Clicked += (_, _) =>
		{
			shell.LoggedIn();
		};

		Content = new VerticalStackLayout
		{
			Children = { button }
		};
	}
}

public class _28708Page1 : ContentPage
{
	public _28708Page1()
	{
		Title = "Page 1";
		Content = new VerticalStackLayout
		{
			Children =
				{
					new Label
					{
						Text = "First Page",
						AutomationId = "label"
					}
				}
		};
	}
}

public class _28708Page2 : ContentPage
{
	public _28708Page2()
	{
		Title = "Page 2";
		Content = new VerticalStackLayout
		{
			Children = { new Label { Text = "Second Page" } }
		};
	}
}