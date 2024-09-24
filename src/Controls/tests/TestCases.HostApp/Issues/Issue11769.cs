using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
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

#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			Device.InvokeOnMainThreadAsync(() =>
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
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
		}
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11769, "[Bug] Shell throws exception when delay adding Shell Section", issueTestNumber: 1)]
	public class Issue11769_DelayedShellSection : TestShell
	{
		protected override void Init()
		{
			Items.Add(new FlyoutItem());
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			Device.InvokeOnMainThreadAsync(() =>
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
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
		}
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 11769, "[Bug] Shell throws exception when delay adding Shell Item", issueTestNumber: 0)]
	public class Issue11769_DelayedShellItem : TestShell
	{
		protected override void Init()
		{
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			Device.InvokeOnMainThreadAsync(() =>
			{
				var page = AddBottomTab("Flyout Item");
				page.Content = new Label()
				{
					Text = "Success"
				};
			});
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
		}
	}
}