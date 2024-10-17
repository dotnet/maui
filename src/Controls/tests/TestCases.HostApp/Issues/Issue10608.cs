namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 10608, "[Bug] [Shell] [iOS] Locked flyout causes application to freezes when quickly switching between tabs", PlatformAffected.iOS)]
public class Issue10608 : TestShell
{
	public Issue10608()
	{
	}

	void AddPage(string title)
	{
		var page = CreateContentPage<FlyoutItem>(title);

		page.Content = new Grid()
		{
			Children =
			{
				new ScrollView()
				{
					Content =
						new StackLayout()
						{
							Children =
							{
								new Button()
								{
									Text = "Learn More",
									Margin = new Thickness(0,10,0,0),
									BackgroundColor = Colors.Purple,
									TextColor = Colors.White,
									AutomationId = "LearnMoreButton"
								}
							}
						}
				}
			}
		};
	}

	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Locked;

		AddPage("Click");
		AddPage("Between");
		AddPage("These Flyouts");
		AddPage("Really Fast");
		AddPage("If it doesn't");
		AddPage("Lock test has passed");

		int i = 0;
		foreach (var item in Items)
		{
			item.Items[0].AutomationId = $"FlyoutItem{i}";
			item.Items[0].Items.Add(new ContentPage()
			{
				Title = "Page"
			});

			i++;
		}

		Items.Add(new MenuItem()
		{
			Text = "Let me click for you",
			AutomationId = $"FlyoutItem{i}",
			Command = new Command(async () =>
			{
				for (int j = 0; j < 5; j++)
				{
					CurrentItem = Items[0].Items[0];
					await Task.Delay(10);
					CurrentItem = Items[1].Items[0];
					await Task.Delay(10);
				}

				CurrentItem = Items[0].Items[0];
			})
		});

		Items[0].Items[0].Items[0].Title = "Tab 1";
		Items[0].Items[0].Items[0].AutomationId = "Tab1AutomationId";
		Items[1].Items[0].Items[0].Title = "Tab 2";
		Items[1].Items[0].Items[0].AutomationId = "Tab2AutomationId";

		Items[0].FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems;
		Items[1].FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems;
	}
}
