namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24741, "Unable to select tab after backing out of page and returning", PlatformAffected.UWP)]
	public class Issue24741 : NavigationPage
	{
		public Issue24741() : base(new TestPage())
		{
		}

		public class TestPage : TestContentPage
		{
			protected override void Init()
			{
				Title = "Issue 24741";

				var navigationButton = new Button
				{
					AutomationId = "NavigateButton",
					Text = "Navigate to TabbedPage"
				};

				navigationButton.Clicked += (s, e) =>
				{
					Navigation.PushAsync(new Issue24741TabbedPage());
				};

				var content = new VerticalStackLayout
				{
					navigationButton
				};

				Content = content;
			}
		}
	}
}