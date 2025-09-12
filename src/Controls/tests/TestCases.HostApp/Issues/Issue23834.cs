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
		var changeButton = new Button
		{
			Text = "Change First Item's IsVisible",
			AutomationId = "button"
		};

		changeButton.Clicked += (s, e) =>
		{
			Shell.Current.Items[0].IsVisible = true;
		};

		layout.Children.Add(changeButton);

		Content = layout;
	}
}