namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22719_1, "The FlyoutPage flyout remains locked when changing the orientation from landscape to portrait with SplitOnLandscape", PlatformAffected.macOS)]
	public partial class Issue22719_1 : FlyoutPage, IFlyoutPageController
	{
		public Issue22719_1()
		{
			InitializeComponent();

			FlyoutLayoutBehavior = FlyoutLayoutBehavior.SplitOnLandscape;
		}

		//  iPhone by default doesn't support split modes so we set this to validate split mode behaviors 
		bool IFlyoutPageController.ShouldShowSplitMode
		{
			get
			{
				if (DeviceDisplay.Current.MainDisplayInfo.Orientation == DisplayOrientation.Landscape)
					return true;

				return false;
			}
		}
	}
}