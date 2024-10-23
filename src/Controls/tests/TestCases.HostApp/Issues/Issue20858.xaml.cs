namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 20858, "FlyoutPage Android app crashing on orientation change when flyout is open", PlatformAffected.Android)]
	public partial class Issue20858 : FlyoutPage, IFlyoutPageController
	{
		public Issue20858()
		{
			InitializeComponent();
		}

		public void OpenFlyout_Clicked(object sender, EventArgs e)
		{
			IsPresented = true;
		}

		bool IFlyoutPageController.ShouldShowSplitMode => FlyoutLayoutBehavior == FlyoutLayoutBehavior.Split;
	}
}