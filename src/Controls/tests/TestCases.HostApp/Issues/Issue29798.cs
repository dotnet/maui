namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29798, "Tab becomes blank after specific navigation pattern", PlatformAffected.iOS)]
public class Issue29798 : Shell
{
	public Issue29798()
	{
		Routing.RegisterRoute(nameof(Issue29798Page), typeof(Issue29798Page));

		Items.Add(new ShellContent()
		{
			Title = "Tab1",
			Content = new ContentPage()
			{
				Title = "Test",
				Content = new Button
				{
					Text = "Go to Page 2",
					AutomationId = "GotoPage2",
					Command = new Command(async () => await Current.GoToAsync(nameof(Issue29798Page)))
				}
			}
		});

		Items[0].Items.Add(new ShellContent()
		{
			Content = new ContentPage()
		});
	}
}

public class Issue29798Page : ContentPage
{
	public Issue29798Page()
	{
		Title = "Page 2";
		Content = new StackLayout
		{
			Children =
				{
					new Label
					{
						AutomationId = "Page2Label",
						 Text = "Welcome to Page 2"
					}
				}
		};
	}
}