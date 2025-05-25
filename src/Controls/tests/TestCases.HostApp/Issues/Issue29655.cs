namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29655, "BackButtonBehavior Clicked event does not exist", PlatformAffected.iOS | PlatformAffected.Android)]
public class Issue29655 : Shell
{
	public Issue29655()
	{
		Routing.RegisterRoute(nameof(Issue29655DetailPage), typeof(Issue29655DetailPage));

		Items.Add(new ShellContent
		{
			Title = "Page 1",
			Content = new ContentPage
			{
				Title = "Page 1",
				Content = new StackLayout
				{
					Children =
					{
						new Button
						{
							Text = "Go to Detail Page",
							AutomationId= "GoToDetailPage",
							Command = new Command(() => Shell.Current.GoToAsync(nameof(Issue29655DetailPage)))
						}
					}
				}
			}
		});
	}
}

public class Issue29655DetailPage : ContentPage
{
	public Issue29655DetailPage()
	{
		Label successLabel = new Label
		{
			Text = "Click event works",
			AutomationId = "SuccessLabel",
			IsVisible = false
		};

		Content = new VerticalStackLayout
		{
			Children =
			{
				new Label { Text = "This is the Detail Page", AutomationId = "DetailPageLabel" },
				successLabel,
			}
		};

		BackButtonBehavior backButtonBehavior = new BackButtonBehavior()
		{
			TextOverride = "Click",
		};

		backButtonBehavior.Clicked += (s, e) => successLabel.IsVisible = true;
		Shell.SetBackButtonBehavior(this, backButtonBehavior);
	}
}