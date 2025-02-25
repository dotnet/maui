using Microsoft.Maui.ApplicationModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 11769, "[Bug] Shell throws exception when delay adding Shell Content", issueTestNumber: 2)]
	public class Issue11769_DelayedShellContent : TestShell
	{
		protected override void Init()
		{
			Items.Add(new FlyoutItem()
			{
				Items =
				{
					new Tab()
				}
			});

			MainThread.InvokeOnMainThreadAsync(() =>
			{
				var shellContent = new ShellContent()
				{
					Content = new ContentPage()
					{
						Content = new Label() { Text = "Success" }
					}
				};

				Items[0].Items[0].Items.Add(shellContent);
			});
		}
	}


	[Issue(IssueTracker.Github, 11769, "[Bug] Shell throws exception when delay adding Shell Section", issueTestNumber: 1)]
	public class Issue11769_DelayedShellSection : TestShell
	{
		protected override void Init()
		{
			Items.Add(new FlyoutItem());
			MainThread.InvokeOnMainThreadAsync(() =>
			{
				var tab = new Tab()
				{
					Items =
					{
						new ShellContent()
						{
							Content = new ContentPage()
							{
								Content = new Label() { Text = "Success" }
							}
						}
					}
				};

				Items[0].Items.Add(tab);
			});
		}
	}


	[Issue(IssueTracker.Github, 11769, "[Bug] Shell throws exception when delay adding Shell Item", issueTestNumber: 0)]
	public class Issue11769_DelayedShellItem : TestShell
	{
		protected override void Init()
		{
			MainThread.InvokeOnMainThreadAsync(() =>
			{
				var page = AddBottomTab("Flyout Item");
				page.Content = new Label()
				{
					Text = "Success"
				};
			});
		}
	}
}