namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 6458, "[Android] Fix load TitleIcon on non app compact", PlatformAffected.Android)]
	public class Issue6458 : TestNavigationPage // or TestFlyoutPage, etc ...
	{
		protected override void Init()
		{
			var contentPage = new ContentPage
			{
				Content = new Label
				{
					AutomationId = "IssuePageLabel",
					Text = "Make sure you run this on Non AppCompact Activity"
				}
			};

			NavigationPage.SetTitleIconImageSource(contentPage,
				new FileImageSource
				{
					File = "bank.png",
					AutomationId = "banktitleicon"
				});

			this.PushAsync(contentPage);
		}
	}
}