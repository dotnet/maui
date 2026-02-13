namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 32275, "SafeAreaEdges should be handled by the FlyoutContentTemplate", PlatformAffected.Android)]
public class Issue32275_Template : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Flyout;

		var page = new Issue32275_Template_ContentPage();

		AddFlyoutItem(page, "Flyout Item");


		FlyoutContentTemplate = new DataTemplate(() =>
		{
			var stackLayout = new StackLayout() { BackgroundColor = Colors.Gray, SafeAreaEdges = SafeAreaEdges.All };

			for (int i = 0; i < 50; i++)
			{
				var label = new Label
				{
					Text = $"Flyout Item {i + 1}",
					FontSize = 24,
				};

				stackLayout.Children.Add(label);
			}
			return stackLayout;
		});
	}

	public class Issue32275_Template_ContentPage : TestContentPage
	{
		protected override void Init()
		{
			Title = "Issue 32275";
			Content = new StackLayout
			{
				Children =
				{
					new Label { AutomationId = "Issue32275_Template_Label", Text = "Open the flyout menu in landscape mode on a device with a display notch or status bar. The flyout menu should not collide with the notch or status bar. The default SafeAreaPadding should not be applied. It should be handled by the template itself." }
				}
			};
		}
	}
}


