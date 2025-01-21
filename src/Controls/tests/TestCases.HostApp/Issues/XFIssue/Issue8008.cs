namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8008, "Removing Shell Item can cause Shell to try and set a MenuItem as the default visible item")]

public class Issue8008 : TestShell
{
	ShellItem item1;
	protected override void Init()
	{
		item1 = AddContentPage();

		item1.Title = "Not Visible";
		Items.Add(new MenuShellItem(new MenuItem()
		{
			Text = "Menu Item",
			Command = new Command(() =>
			{
				throw new Exception("I shouldn't execute after removing an item");
			})
		}));

		var item2 = AddContentPage(new ContentPage()
		{
			Content = new StackLayout()
			{
				new Label()
				{
					Text = "If you are reading this then this test has passed",
					AutomationId = "Success"
				}
			}
		});

		item2.Title = "Visible After Remove";

		MainThread.BeginInvokeOnMainThread(() =>
		{
			this.Items.Remove(item1);
		});


	}
}
