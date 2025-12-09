namespace Maui.Controls.Sample.iOS;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
	public override UIWindow? Window { get; set; }

	public override bool FinishedLaunching(UIApplication application, NSDictionary? launchOptions) => true;

	public override UISceneConfiguration GetConfiguration(UIApplication application, UISceneSession connectingSceneSession, UISceneConnectionOptions options) =>
		Enumerable.FirstOrDefault<NSUserActivity>(options.UserActivities)?.ActivityType == "NewTaskWindow"
			? new UISceneConfiguration("New Task Configuration", UIWindowSceneSessionRole.Application)
			: new UISceneConfiguration("Default Configuration", UIWindowSceneSessionRole.Application);
}
