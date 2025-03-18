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

		// Placing a Label directly within the flyout content centers it on Windows, complicating offset testing.
		// Wrapping the Label in a StackLayout within the flyout content enables testing offset values on Windows.
		var stackLayout = new VerticalStackLayout();
		stackLayout.Children.Add(new Label()
		{
			AutomationId = "LabelContent",
			Text = "Only Label"
		});
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
						FlyoutContent = stackLayout;
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
