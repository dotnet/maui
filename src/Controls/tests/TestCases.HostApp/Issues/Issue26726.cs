namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 26726, "Flyout Icon Positioned Incorrectly in RTL mode", PlatformAffected.iOS)]
	class Issue26726 : FlyoutPage
	{
		public Issue26726()
		{
			Flyout = new ContentPage { Title = "Flyout" };
			Detail = new NavigationPage(new ContentPage
			{
				Title = "Detail",
				Content = new StackLayout
				{
					Children = {
						new Button
						{
							Text = "Set RightToLeft",
							Command = new Command(() => FlowDirection = FlowDirection.RightToLeft),
							AutomationId = "ShowRightToLeft"
						}
					}
				}
			});
		}

	}
}
