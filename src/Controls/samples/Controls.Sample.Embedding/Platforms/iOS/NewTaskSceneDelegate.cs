namespace Maui.Controls.Sample.iOS;

[Register(nameof(NewTaskSceneDelegate))]
public class NewTaskSceneDelegate : UIWindowSceneDelegate
{
	public override UIWindow? Window { get; set; }

	public override void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
	{
		if (scene is not UIWindowScene windowScene)
			return;

		var window = new UIWindow(windowScene);
		window.RootViewController = new NewTaskViewController();
		window.WindowScene!.Title = "New Task";
		window.MakeKeyAndVisible();

		Window = window;
	}
}
