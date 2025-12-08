namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 28130, "[Windows] Flyout Menu Icon disappears from Window Title Bar after Navigation", PlatformAffected.UWP)]
	public class Issue28130_Flyout : TestFlyoutPage
	{
		NavigationPage _navigationPage;
		string pageTitle = "Issue28130";
		protected override void Init()
		{
			Flyout = CreateFlyoutContent();
			FlyoutLayoutBehavior = FlyoutLayoutBehavior.Popover;
			Detail = _navigationPage = new NavigationPage(new Issue28130_DetailPage(pageTitle) { AutomationId = pageTitle });
		}

		ContentPage CreateFlyoutContent()
		{
			var flyoutContent = new ContentPage() { Title = "Menu" };
			var layout = new VerticalStackLayout();
			var navigateButton = new Button() { Text = "Navigate to Page 2", AutomationId = "NavigateButton" };
			navigateButton.Clicked += (s, e) => _navigationPage.PushAsync(new Issue28130_Page1());
			layout.Add(navigateButton);
			flyoutContent.Content = layout;
			return flyoutContent;
		}
	}

	public class Issue28130_Page1 : TestContentPage
	{
		protected override void Init()
		{
			Content = new VerticalStackLayout()
			{
				new Label()
				{
					Text = "Tap flyout",
					AutomationId="newPageLabel"
				}
			};
		}
	}

	public class Issue28130_DetailPage : ContentPage
	{
		public Issue28130_DetailPage(string title)
		{
			Title = title;
			Content = new VerticalStackLayout()
			{
				new Label()
				{
					Text = "Once Loaded, Tap the flyout",
					AutomationId = "detailLabel"
				}
			};
		}
	}
}