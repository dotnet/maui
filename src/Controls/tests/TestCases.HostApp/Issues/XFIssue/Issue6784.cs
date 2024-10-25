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

		AddBottomTab("Tab 1");
		AddBottomTab("Tab 2");
		AddBottomTab("Tab 3");
		AddBottomTab("Tab 4").AutomationId = "Tab 4 Content";
		var contentPage5 = AddBottomTab("Tab 5");
		AddBottomTab("Tab 6");

		shellItem.PropertyChanged += (sender, e) =>
		{
			if (e.PropertyName == CurrentItemProperty.PropertyName)
			{
				if (((ShellItem)sender).CurrentItem.AutomationId == "Tab 5")
				{
					contentPage5.Content = new Label()
					{
						Text = "Success"
					};
				}
			}
		};
	}
}