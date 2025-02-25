namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.None, 0, "Shell Flyout Behavior",
	PlatformAffected.All)]

public class FlyoutBehaviorShell : TestShell
{
	BackButtonBehavior _behavior;
	const string title = "Basic Test";
	const string FlyoutItem = "Flyout Item";
	const string EnableFlyoutBehavior = "EnableFlyoutBehavior";
	const string DisableFlyoutBehavior = "DisableFlyoutBehavior";
	const string LockFlyoutBehavior = "LockFlyoutBehavior";
	const string OpenFlyout = "OpenFlyout";
	const string EnableBackButtonBehavior = "EnableBackButtonBehavior";
	const string DisableBackButtonBehavior = "DisableBackButtonBehavior";

	protected override void Init()
	{
		_behavior = new BackButtonBehavior();
		var page = GetContentPage(title);
		Shell.SetBackButtonBehavior(page, _behavior);
		AddContentPage(page).Title = FlyoutItem;
		Shell.SetFlyoutBehavior(this.CurrentItem, FlyoutBehavior.Disabled);
	}

	ContentPage GetContentPage(string title)
	{
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		ContentPage page = new ContentPage()
		{
			Title = title,
			Content = new StackLayout()
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Colors.Red,
				Children =
				{
					new Button()
					{
						Text = "Enable Flyout Behavior",
						Command = new Command(() =>
						{
							Shell.SetFlyoutBehavior(this.CurrentItem, FlyoutBehavior.Flyout);
							this.FlyoutIsPresented = false;
						}),
						AutomationId = EnableFlyoutBehavior
					},
					new Button()
					{
						Text = "Disable Flyout Behavior",
						Command = new Command(() =>
						{
							Shell.SetFlyoutBehavior(this.CurrentItem, FlyoutBehavior.Disabled);
							this.FlyoutIsPresented = false;
						}),
						AutomationId = DisableFlyoutBehavior
					},
					new Button()
					{
						Text = "Lock Flyout Behavior",
						Command = new Command(() =>
						{
							Shell.SetFlyoutBehavior(this.CurrentItem, FlyoutBehavior.Locked);
						}),
						AutomationId = LockFlyoutBehavior
					},
					new StackLayout()
					{
						VerticalOptions = LayoutOptions.CenterAndExpand
					},
					new Button()
					{
						Text = "Open Flyout",
						Command = new Command(() =>
						{
							this.FlyoutIsPresented = true;
						}),
						AutomationId = OpenFlyout,
						VerticalOptions = LayoutOptions.End
					},
					new Button()
					{
						Text = "Enable Back Button Behavior",
						Command = new Command(() =>
						{
							_behavior.IsEnabled = true;
						}),
						AutomationId = EnableBackButtonBehavior,
						VerticalOptions = LayoutOptions.End

					},
					new Button()
					{
						Text = "Disable Back Button Behavior",
						Command = new Command(() =>
						{
							_behavior.IsEnabled = false;
						}),
						AutomationId = DisableBackButtonBehavior,
						VerticalOptions = LayoutOptions.End
					}
				}
			}
		};
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

		return page;
	}
}
