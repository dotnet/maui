namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 22719, "FlyoutPage flyout disappears when maximizing the window on the mac platform", PlatformAffected.macOS)]
	public partial class Issue22719 : FlyoutPage, IFlyoutPageController
	{
		public Issue22719()
		{
			InitializeComponent();
#if WINDOWS
			FlyoutLayoutBehavior = FlyoutLayoutBehavior.SplitOnLandscape;
#else
			FlyoutLayoutBehavior = FlyoutLayoutBehavior.SplitOnPortrait;
#endif
		}

		//  iPhone by default doesn't support split modes so we set this to validate split mode behaviors 
		bool IFlyoutPageController.ShouldShowSplitMode
		{
			get
			{
				var orientation = DeviceDisplay.Current.MainDisplayInfo.Orientation;
#if WINDOWS
				return orientation == DisplayOrientation.Landscape;
#else
				return orientation == DisplayOrientation.Portrait;
#endif
			}
		}
	}
}