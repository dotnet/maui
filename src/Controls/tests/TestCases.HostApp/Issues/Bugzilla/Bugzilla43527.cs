namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 43527, "[UWP] Detail title does not update when wrapped in a NavigationPage", PlatformAffected.WinRT)]
public class Bugzilla43527 : TestFlyoutPage
{
	protected override void Init()
	{
		Flyout = new ContentPage
		{
			Title = "Flyout",
			BackgroundColor = Colors.Red
		};

		Detail = new NavigationPage(new TestPage());
	}

	class TestPage : ContentPage
	{
		public TestPage()
		{
			Title = "Test Page";
			AutomationId = "Test Page";

			Content = new StackLayout
			{
				Children = {
					new Label { Text = "Hello Page" },
					new Button { Text = "Change Title", AutomationId = "Change Title", Command = new Command(() =>
					{
						Title = "New Title";
					})
					}
				}
			};
		}
	}
}