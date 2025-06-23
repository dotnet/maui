namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24547, "[Windows] FlyoutPage ShouldShowToolbarButton when overridden to return false, still shows button in title bar", PlatformAffected.UWP)]
	public class Issue24547PopoverPage : FlyoutPage
	{
		public Issue24547PopoverPage()
		{
			FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;

			Flyout = new ContentPage
			{
				Title = "Flyout",
				BackgroundColor = Colors.Red,
				Content = new StackLayout
				{
					Children = {
						new Label { Text = "Flyout" }
					}
				}
			};

			ContentPage contentPage = new ContentPage
			{
				BackgroundColor = Colors.Green,
				Title = "Detail",
				Content = new StackLayout
				{
					Children =
					{
						CreateDetailButton()
					}
				}
			};

			Detail = new NavigationPage(contentPage);
			Button button = new Button() { Text = "Menu", AutomationId = "MenuButton" };
			button.Clicked += (s, e) => IsPresented = true;
			NavigationPage.SetTitleView(contentPage, button);
		}

		private Button CreateDetailButton()
		{
			Button button = new Button
			{
				Text = "Detail",
				AutomationId = "DetailButton"
			};

			return button;
		}

		public override bool ShouldShowToolbarButton()
		{
			return false;
		}
	}
}