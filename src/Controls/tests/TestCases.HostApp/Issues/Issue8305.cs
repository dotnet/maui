namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8305, "Add Shell Badge support", PlatformAffected.All)]
public class Issue8305 : TestShell
{
	protected override void Init()
	{
		var tab1Content = new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children =
				{
					new Label
					{
						Text = "Shell Badge Test",
						AutomationId = "HeaderLabel",
						FontSize = 24
					},
					new Button
					{
						Text = "Set Tab2 Badge to 5",
						AutomationId = "SetBadgeButton",
						Command = new Command(() =>
						{
							Items[0].Items[1].BadgeText = "5";
							Items[0].Items[1].BadgeColor = Colors.Red;
						})
					},
					new Button
					{
						Text = "Set Tab2 Badge to New",
						AutomationId = "SetTextBadgeButton",
						Command = new Command(() =>
						{
							Items[0].Items[1].BadgeText = "New";
							Items[0].Items[1].BadgeColor = Colors.Blue;
						})
					},
					new Button
					{
						Text = "Clear Tab2 Badge",
						AutomationId = "ClearBadgeButton",
						Command = new Command(() =>
						{
							Items[0].Items[1].BadgeText = null;
							Items[0].Items[1].BadgeColor = null;
						})
					},
					new Button
					{
						Text = "Set Tab3 Badge to 99+",
						AutomationId = "SetTab3BadgeButton",
						Command = new Command(() =>
						{
							Items[0].Items[2].BadgeText = "99+";
							Items[0].Items[2].BadgeColor = Colors.Orange;
						})
					}
				}
			}
		};

		var tab1 = new Tab { Title = "Home", Icon = "groceries.png", BadgeText = "New", BadgeColor = Colors.Blue };
		tab1.Items.Add(new ShellContent { Title = "Home", Content = tab1Content });

		var tab2 = new Tab { Title = "Messages", Icon = "dotnet_bot.png", BadgeText = "3", BadgeColor = Colors.Red };
		tab2.Items.Add(new ShellContent { Title = "Messages", Content = new ContentPage
		{
			Content = new Label { Text = "Messages Tab", AutomationId = "MessagesLabel", Padding = 20 }
		}});

		var tab3 = new Tab { Title = "Alerts", Icon = "groceries.png" };
		tab3.Items.Add(new ShellContent { Title = "Alerts", Content = new ContentPage
		{
			Content = new Label { Text = "Alerts Tab", AutomationId = "AlertsLabel", Padding = 20 }
		}});

		var tabBar = new TabBar();
		tabBar.Items.Add(tab1);
		tabBar.Items.Add(tab2);
		tabBar.Items.Add(tab3);
		Items.Add(tabBar);
	}
}
