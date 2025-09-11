namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6101, "Picker items do not appear when tapping on the picker while using PushModalAsync for navigation", PlatformAffected.macOS)]

public class Issue6101Shell : Shell
{
	public Issue6101Shell()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;

		ShellContent shellContent = new ShellContent
		{
			ContentTemplate = new DataTemplate(typeof(Issue6101)),
			Route = "MainPage"
		};

		Items.Add(shellContent);
	}

	public class Issue6101 : ContentPage
	{
		public Issue6101()
		{
			Content = new StackLayout
			{
				Children =
				{
					new Button
					{
						Text = "Navigate to page that opens modal",
						AutomationId = "Button",
						Command = new Command(() => Navigation.PushModalAsync(new Issue6101Page1()))
					}
				}
			};
		}
	}

	public class Issue6101Page1 : ContentPage
	{
		public Issue6101Page1()
		{
			Content = new VerticalStackLayout()
			{
				Children =
				{
					new Picker
					{
						AutomationId = "Picker",
						ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3" } ,
						SelectedIndex = 0,
					}
				}
			};
		}
	}
}








