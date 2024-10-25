namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Shell Flyout Content Offsets Correctly",
	   PlatformAffected.All)]
public class ShellFlyoutContentOffset : TestShell
{
	public ShellFlyoutContentOffset()
	{
	}

	protected override void Init()
	{
		AddFlyoutItem(CreateContentPage(), "Item 1");
		FlyoutFooter = new Button()
		{
			HeightRequest = 200,
			AutomationId = "CloseFlyout",
			Command = new Command(() => FlyoutIsPresented = false),
			Text = "Close Flyout"
		};
	}

	ContentPage CreateContentPage()
	{
		var layout = new StackLayout()
		{
			new Label()
			{
				AutomationId = "PageLoaded",
				Text = "Toggle through the 3 variations of flyout content and verify they all offset the same. Toggle the Header/Footer and then verify again",
			},
			new Button()
			{
				Text = "Toggle Flyout Content",
				Command = new Command(() =>
				{
					if (FlyoutContent is ScrollView)
						FlyoutContent = null;
					else if (FlyoutContent == null)
						FlyoutContent = new Label()
						{
							AutomationId = "LabelContent",
							Text = "Only Label"
						};
					else
						FlyoutContent = new ScrollView()
						{
							Content = new Label()
							{
								AutomationId = "ScrollViewContent",
								Text = "Label inside ScrollView"
							}
						};
				}),
				AutomationId = "ToggleFlyoutContent"
			},
			new Button()
			{
				Text = "Toggle Header",
				Command = new Command(() =>
				{
					if (FlyoutHeader == null)
					{
						FlyoutHeader =
							new BoxView()
							{
								BackgroundColor = Colors.Blue,
								HeightRequest = 50
							};
					}
					else
					{
						FlyoutHeader = FlyoutFooter = null;
					}
				}),
				AutomationId = "ToggleHeader"
			}
		};

		return new ContentPage()
		{
			Content = layout
		};
	}
}
