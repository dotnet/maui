namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 15173, "Shell Flyout overlay does not resize on device rotation", PlatformAffected.iOS)]
public class Issue15173 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;
		FlyoutBackgroundColor = Colors.White;

		// Create flyout content
		var flyoutContent = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 10,
			BackgroundColor = Colors.LightGray,
			Children =
			{
				new Label 
				{ 
					Text = "Flyout Menu", 
					FontSize = 24, 
					FontAttributes = FontAttributes.Bold, 
					Margin = new Thickness(0, 20, 0, 10) 
				},
				new Button 
				{ 
					Text = "Menu Item 1", 
					BackgroundColor = Colors.White, 
					TextColor = Colors.Black, 
					AutomationId = "MenuItem1" 
				},
				new Button 
				{ 
					Text = "Menu Item 2", 
					BackgroundColor = Colors.White, 
					TextColor = Colors.Black, 
					AutomationId = "MenuItem2" 
				},
				new Button 
				{ 
					Text = "Menu Item 3", 
					BackgroundColor = Colors.White, 
					TextColor = Colors.Black, 
					AutomationId = "MenuItem3" 
				}
			}
		};

		// Set the flyout content using the property
		FlyoutContentTemplate = new DataTemplate(() => flyoutContent);

		// Create main content page
		var mainPage = new ContentPage
		{
			BackgroundColor = Colors.White,
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(30),
				Spacing = 20,
				Children =
				{
					new Label 
					{ 
						Text = "Shell Flyout Rotation Test",
						FontSize = 24,
						FontAttributes = FontAttributes.Bold,
						HorizontalOptions = LayoutOptions.Center,
						TextColor = Colors.Black,
						AutomationId = "PageTitle"
					},
					new Label 
					{ 
						Text = "Instructions:",
						FontSize = 18,
						FontAttributes = FontAttributes.Bold,
						TextColor = Colors.Black,
						Margin = new Thickness(0, 20, 0, 0)
					},
					new Label 
					{ 
						Text = "1. Open the flyout (hamburger menu)",
						FontSize = 16,
						TextColor = Colors.Black
					},
					new Label 
					{ 
						Text = "2. Rotate the device from portrait to landscape",
						FontSize = 16,
						TextColor = Colors.Black
					},
					new Label 
					{ 
						Text = "3. The overlay should cover the entire landscape screen",
						FontSize = 16,
						TextColor = Colors.Black
					}
				}
			}
		};

		AddContentPage(mainPage, "Home");
	}
}
