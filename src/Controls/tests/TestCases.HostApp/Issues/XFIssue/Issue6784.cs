namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6784, "ShellItem.CurrentItem is not set when selecting shell section aggregated in more tab", PlatformAffected.iOS)]

public class Issue6784 : TestShell
{
	protected override void Init()
	{
		var shellItem = new ShellItem()
		{
			Title = "ShellItem"
		};

		Items.Add(shellItem);

		for (int i = 1; i <= 10; i++)
		{
			AddBottomTab($"Tab {i}");
		}
		var contentPage11 = AddBottomTab("Tab 11");
		AddBottomTab("Tab 12");

		shellItem.PropertyChanged += (sender, e) =>
		{
			if (e.PropertyName == CurrentItemProperty.PropertyName)
			{
				if (((ShellItem)sender).CurrentItem.AutomationId == "Tab 11")
				{
					contentPage11.Content = new Label()
					{
						Text = "Success"
					};
				}
			}
		};
	}
}