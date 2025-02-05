namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22719, "FlyoutPage flyout disappears when maximizing the window on the mac platform", PlatformAffected.macOS)]
	public partial class Issue22719 : FlyoutPage, IFlyoutPageController
	{
		public Issue22719()
		{
			InitializeComponent();
			FlyoutLayoutBehavior = FlyoutLayoutBehavior.SplitOnPortrait;

		}

		//  iPhone by default doesn't support split modes so we set this to validate split mode behaviors 
		bool IFlyoutPageController.ShouldShowSplitMode
		{
			get
			{
				if (DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Portrait)
					return true;

				return false;
			}
		}
	}
}