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
		AddBottomTab("Tab 5");
		AddBottomTab("Tab 6");
		AddBottomTab("Tab 7");
		AddBottomTab("Tab 8");
		AddBottomTab("Tab 9");
		AddBottomTab("Tab 10");
		var contentPage5 = AddBottomTab("Tab 11");
		AddBottomTab("Tab 12");

		shellItem.PropertyChanged += (sender, e) =>
		{
			if (e.PropertyName == CurrentItemProperty.PropertyName)
			{
				if (((ShellItem)sender).CurrentItem.AutomationId == "Tab 11")
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