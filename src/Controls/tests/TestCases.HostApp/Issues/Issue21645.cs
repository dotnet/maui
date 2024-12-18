namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 21645, "Android - Navigation Title disappears if Flyout is set to a new window", PlatformAffected.Android | PlatformAffected.UWP)]
	public class Issue21645 : FlyoutPage
	{
		public Issue21645()
		{
			Flyout = new ContentPage
			{
				Title = "Flyout",
				Content = new StackLayout
				{
					Children = 
					{
						new Label { Text = "Flyout" }
					}
				}
			};

			ContentPage contentPage = new ContentPage
			{
				Title = "Detail",
				Content = new StackLayout
				{
					Children = 
					{
						CreateResetButton()
					}
				}
			};

			Detail = new NavigationPage(contentPage);
			Label label = new Label() { Text = "Title", AutomationId = "TitleLabel", VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start, FontSize=32 };
			NavigationPage.SetTitleView(contentPage, label);
		}

		private Button CreateResetButton()
		{
			Button button = new Button
			{
				Text = "Reset Flyout",
				AutomationId = "ResetFlyoutButton"
			};

			button.Clicked += OnFlyoutResetButtonClicked;
			return button;
		}

		private void OnFlyoutResetButtonClicked(object sender, EventArgs e)
		{
			Flyout = new ContentPage() { Title = "Flyout"};
		}
	}
}