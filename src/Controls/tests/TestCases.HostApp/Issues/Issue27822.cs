namespace Controls.TestCases.HostApp.Issues;

[Issue(IssueTracker.Github, 27822, "LinearGradientBrush in Shell FlyoutBackground not working", PlatformAffected.iOS)]

public class Issue27822 : ContentPage
{
	public Issue27822()
	{
		Title = "Issue 27822 Main Page";

		var navigateButton = new Button
		{
			AutomationId = "navigateButton",
			Text = "Go to Flyout Page"
		};

		navigateButton.Clicked += (s, e) =>
		{
			Application.Current.MainPage = new Issue27822FlyoutPage();
		};

		Content = new VerticalStackLayout
		{
			VerticalOptions = LayoutOptions.Center,
			Children =
				{
					new Label
					{
						Text = "This is the Main Page",
						HorizontalOptions = LayoutOptions.Center
					},
					navigateButton
				}
		};
	}
}

public class Issue27822FlyoutPage : Shell
{
	public Issue27822FlyoutPage()
	{
		FlyoutBehavior = FlyoutBehavior.Locked;
		FlyoutBackground = new LinearGradientBrush
		{
			StartPoint = new Point(0, 0),
			EndPoint = new Point(0, 1),
			GradientStops =
				{
					new GradientStop { Color = Colors.Green, Offset = 0.0f },
					new GradientStop { Color = Colors.Yellow, Offset = 0.5f }
				}
		};

		var mainPage = new ContentPage
		{
			Title = "Flyout Home",
			Content = new VerticalStackLayout
			{
				Children = {
						new Label
						{
							AutomationId = "welcomeLabel",
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center,
							Text = "Test passes if you can see the gradient background in the flyout."
						}
					}
			}
		};

		// Add Flyout Item
		Items.Add(new FlyoutItem
		{
			Title = "Home",
			Items =
				{
					new Tab
					{
						Title = "Main Tab",
						Items =
						{
							new ShellContent { Content = mainPage }
						}
					}
				}
		});
	}
}