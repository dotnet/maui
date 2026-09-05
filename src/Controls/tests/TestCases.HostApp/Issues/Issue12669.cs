namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 12669, "Changing Content property of ShellContent doesn't change visual content", PlatformAffected.All)]
public class Issue12669 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;

		var shellContent = new ShellContent { Title = "Test" };

		var changeButton = new Button
		{
			Text = "Change Content",
			AutomationId = "ChangeContentButton"
		};

		shellContent.Content = new ContentPage
		{
			Content = new VerticalStackLayout
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					new Label
					{
						Text = "Original Content",
						AutomationId = "OriginalContent"
					},
					changeButton
				}
			}
		};

		changeButton.Clicked += (s, e) =>
		{
			shellContent.ContentTemplate = null;
			shellContent.Content = new ContentPage
			{
				Content = new VerticalStackLayout
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					Children =
					{
						new Label
						{
							Text = "Content Changed",
							AutomationId = "NewContent"
						}
					}
				}
			};
		};

		var tab = new Tab { Title = "Main" };
		tab.Items.Add(shellContent);
		Items.Add(tab);
	}
}
