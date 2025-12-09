namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 20685, "MenuBarItem Commands not working on Mac Catalyst",
	PlatformAffected.All)]
public class Issue20685 : TestShell
{
	protected override void Init()
	{
		var resultLabel = new Label
		{
			Text = "No action performed yet",
			FontSize = 18,
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "ResultLabel"
		};

		var menuItems = new MenuBarItem { Text = "MenuItems" };


		// 1. Menu item using Clicked event
		var clickedItem = new MenuFlyoutItem
		{
			Text = "Test Clicked Event",
			AutomationId = "ClickedEventItem"
		};
		clickedItem.Clicked += (s, e) => resultLabel.Text = "Clicked event handler executed";
		menuItems.Add(clickedItem);

		// 2. Menu item using Command
		var commandItem = new MenuFlyoutItem
		{
			Text = "Test Command",
			AutomationId = "CommandItem",
			Command = new Command(() => resultLabel.Text = "Command executed")
		};
		menuItems.Add(commandItem);

		// 3. Menu item using Command with parameter
		var parameterItem = new MenuFlyoutItem
		{
			Text = "Test Command Parameter",
			AutomationId = "CommandWithParamItem",
			Command = new Command<string>(param => resultLabel.Text = $"Command executed with parameter: {param}"),
			CommandParameter = "Test Parameter"
		};
		menuItems.Add(parameterItem);

		MenuBarItems.Add(menuItems);

		var contentPage = new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Padding = new Thickness(20),
				Spacing = 20,
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Center,
				Children =
					{
						new Label
						{
							Text = "MenuFlyoutItem Test",
							FontSize = 24,
							HorizontalOptions = LayoutOptions.Center
						},
						resultLabel,
						new Label
						{
							Text = "Use the menu bar at the top to test the commands",
							HorizontalOptions = LayoutOptions.Center,
							HorizontalTextAlignment = TextAlignment.Center
						}
					}
			}
		};

		AddContentPage(contentPage);
	}
}