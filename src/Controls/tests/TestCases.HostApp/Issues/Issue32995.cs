namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32995, "TabBarDisabledColor not applied to disabled tabs on iOS", PlatformAffected.iOS)]
public class Issue32995 : Shell
{
	public Issue32995()
	{
		var tab2 = new Tab
		{
			Title = "Tab2",
			Icon = "coffee.png",
			AutomationId = "Tab2",
			IsEnabled = false,
			Items =
			{
				new ShellContent
				{
					Content = new ContentPage
					{
						Content = new Label
						{
							Text = "Tab2 Content",
							AutomationId = "Tab2Label",
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center
						}
					}
				}
			}
		};

		var tab1 = new Tab
		{
			Title = "Tab1",
			Icon = "coffee.png",
			Items =
			{
				new ShellContent
				{
					Content = new ContentPage
					{
						Content = new VerticalStackLayout
						{
							Padding = 20,
							Spacing = 15,
							Children =
							{
								new Label { Text = "Tab1", FontAttributes = FontAttributes.Bold },
								new Button
								{
									Text = "Enable Tab2",
									AutomationId = "EnableButton",
									Command = new Command(() => tab2.IsEnabled = true)
								},
								new Button
								{
									Text = "Disable Tab2",
									AutomationId = "DisableButton",
									Command = new Command(() => tab2.IsEnabled = false)
								}
							}
						}
					}
				}
			}
		};

		Items.Add(tab1);
		Items.Add(tab2);

		// Set TabBarDisabledColor using attached property
		Shell.SetTabBarDisabledColor(this, Colors.Green);
	}
}
