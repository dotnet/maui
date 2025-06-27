namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28485, "Back-navigation with swipe-back navigates back twice", PlatformAffected.iOS)]
public class Issue28485 : Shell
{
	public Issue28485()
	{
		Routing.RegisterRoute("page2", typeof(Issue28485Page2));
		Routing.RegisterRoute("page3", typeof(Issue28485Page3));

		var page1 = new ShellContent
		{
			Title = "Page 1",
			Content = new ContentPage()
			{
				Content = new Button
				{
					Text = "Go to Page 2",
					AutomationId = "GotoPage2",
					Command = new Command(async () =>
					{
						await Shell.Current.GoToAsync("page2", false);
					})
				}
			}
		};

		Items.Add(page1);
	}

	class Issue28485Page2 : ContentPage
	{
		public Issue28485Page2()
		{
			Title = "Page 1";
			Content = new StackLayout
			{
				Children =
				{
					new Label { Text = "Welcome to Page 2", AutomationId="Page2Label" },
					new Button
					{
						Text = "Go to Page 3",
						AutomationId = "GotoPage3",
						Command = new Command(async () =>
						{
							await Shell.Current.GoToAsync("page3", false);
						})
					}
				}
			};
		}
	}

	class Issue28485Page3 : ContentPage
	{
		public Issue28485Page3()
		{
			Title = "Page 3";
			Content = new Label { Text = "Welcome to Page 3", AutomationId = "Page3Label" };
		}
	}
}