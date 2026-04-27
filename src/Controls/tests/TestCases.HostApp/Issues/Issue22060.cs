namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 22060, "Flyout icon and content page title disappeared after focusing on the search handler", PlatformAffected.iOS)]
public class Issue22060Shell : Shell
{
	public Issue22060Shell()
	{
		this.FlyoutBehavior = FlyoutBehavior.Flyout;

		var shellContent = new ShellContent
		{
			Title = "Home",
			Route = "MainPage",
			Content = new Issue22060() { Title = "Home" }
		};

		Items.Add(shellContent);
	}

	class Issue22060 : ContentPage
	{
		public Issue22060()
		{
			Shell.SetSearchHandler(this, new SearchHandler
			{
				AutomationId = "searchHandler",
				Placeholder = "SearchHandler",
			});

			this.SetBinding(TitleProperty, new Binding("Title", source: Shell.Current));
		}
	}
}