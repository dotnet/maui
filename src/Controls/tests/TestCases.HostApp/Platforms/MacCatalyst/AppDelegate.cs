using Foundation;
using Maui.Controls.Sample.Issues;
using UIKit;

namespace Maui.Controls.Sample.Platform
{
	[Register("AppDelegate")]
	public class AppDelegate : MauiUIApplicationDelegate
	{
		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

		public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
		{

#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<Issue5503>(this, Issue5503.ChangeUITableViewAppearanceBgColor, (s) =>
			{
				UITableView.Appearance.BackgroundColor = UITableView.Appearance.BackgroundColor == null ? UIColor.Red : null;
			});
#pragma warning restore CS0618 // Type or member is obsolete

			return base.FinishedLaunching(uiApplication, launchOptions);
		}
	}
}