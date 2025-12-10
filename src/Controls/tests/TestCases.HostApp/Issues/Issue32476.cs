namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32476, "Binding RTL FlowDirection in Shell causes Flyout MenuIcon and native window controls to overlap", PlatformAffected.UWP)]
public class Issue32476 : Shell
{
	public Issue32476()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;
		var button = new Button
		{
			Text = "Toggle FlowDirection to RTL",
			AutomationId = "ToggleButton"
		};

		button.Clicked += (s, e) =>
		{
			FlowDirection = FlowDirection == FlowDirection.LeftToRight
				? FlowDirection.RightToLeft
				: FlowDirection.LeftToRight;
		};

		var contentPage = new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				Children =
					{
						new Label
						{
							Text = "Tap the button to toggle FlowDirection to RTL. The Flyout MenuIcon must not overlap with native window controls.",
							LineBreakMode = LineBreakMode.WordWrap
						},
						button
					}
			}
		};
		Items.Add(new FlyoutItem
		{
			Title = "Home",
			Items =
				{
					new ShellContent
					{
						Title = "Main",
						Content = contentPage
					}
				}
		});
	}
}