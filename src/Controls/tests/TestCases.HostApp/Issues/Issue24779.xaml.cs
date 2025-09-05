using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24779, "Android crashing when rotating a flyout page", PlatformAffected.Android)]
	public partial class Issue24779 : FlyoutPage, IFlyoutPageController
	{
		public Issue24779()
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