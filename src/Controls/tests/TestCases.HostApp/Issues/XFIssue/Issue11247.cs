namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 11247,
	"[Bug] Shell FlyoutIsPresented not working if set in \"navigating\" handler",
	PlatformAffected.iOS)]
public class Issue11247 : TestShell
{
	protected override void Init()
	{
		var page = CreateContentPage<FlyoutItem>("FlyoutItem 1");
		CreateContentPage<FlyoutItem>("FlyoutItem 2");

		Items.Add(new MenuItem()
		{
			Text = "Click Me To Close Flyout",
			AutomationId = "CloseFlyout",
			Command = new Command(() =>
			{
				FlyoutIsPresented = false;
			})
		});

		page.Content = new StackLayout()
		{
			Children =
			{
				new Label()
				{
					Text = "If the flyout wasn't open when this test started the test has failed"
				},
				new Label()
				{
					Text = "Now, Open the Flyout and Click on FlyoutItem 2. Nothing should happen and flyout should remain open"
				}
			}
		};
	}

	protected override void OnNavigating(ShellNavigatingEventArgs args)
	{
		base.OnNavigating(args);

		if (args.CanCancel)
		{
			args.Cancel();
		}

		FlyoutIsPresented = true;
	}
}
