namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22719, "FlyoutPage flyout disappears when maximizing the window on the mac platform", PlatformAffected.macOS)]
	public partial class Issue22719 : FlyoutPage, IFlyoutPageController
	{
		public Issue22719()
		{
			InitializeComponent();
		}
	}
}