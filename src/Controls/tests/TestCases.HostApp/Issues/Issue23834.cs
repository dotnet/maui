namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23834, "Flyout Item misbehavior", PlatformAffected.UWP)]
public class Issue23834 : TestShell
{
	protected override void Init()
	{
		var shellContent = new ShellContent()
		{
			ContentTemplate = new DataTemplate(typeof(Issue23834SamplePage))
		};

		this.Items.Add(new FlyoutItem()
		{
			Title = "Flyout Item 1",
			IsVisible = false,
			Items =
			{
				shellContent
			}
		});

		this.Items.Add(new FlyoutItem()
		{
			Title = "Flyout Item 2",
			Items =
			{
				shellContent
			}
		});
	}
}

public class Issue23834SamplePage : ContentPage
{
	public Issue23834SamplePage()
	{
		var layout = new StackLayout();
		var ChangeButton = new Button
		{
			Text = "Change First Item's IsVisible",
			AutomationId = "FirstFlyoutItem",
			Command = new Command(ChangeFlyoutItem)
		};

		layout.Children.Add(ChangeButton);

		Content = layout;
	}

	private void ChangeFlyoutItem()
	{
		var shell = Application.Current.MainPage as TestShell;
		var FlyoutItem = shell.Items[0] as FlyoutItem;
		FlyoutItem.IsVisible = true;
	}
}