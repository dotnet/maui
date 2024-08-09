namespace Maui.Controls.Sample.MacCatalyst;

[Register(nameof(SceneDelegate))]
public class SceneDelegate : UIWindowSceneDelegate
{
	public override UIWindow? Window { get; set; }

	public override void WillConnect(UIScene scene, UISceneSession session, UISceneConnectionOptions connectionOptions)
	{
		// Use this method to optionally configure and attach the UIWindow `window` to the provided UIWindowScene `scene`.
		// If using a storyboard, the `window` property will automatically be initialized and attached to the scene.
		// This delegate does not imply the connecting scene or session are new (see UIApplicationDelegate `GetConfiguration` instead).

		if (scene is not UIWindowScene windowScene)
			return;

		Window = new UIWindow(windowScene);

		var mainVC = new MainViewController();
		var navigationController = new UINavigationController(mainVC);
		navigationController.NavigationBar.PrefersLargeTitles = true;

		Window.RootViewController = navigationController;
		Window.MakeKeyAndVisible();
	}

	public override void DidDisconnect(UIScene scene)
	{
		// Called as the scene is being released by the system.
		// This occurs shortly after the scene enters the background, or when its session is discarded.
		// Release any resources associated with this scene that can be re-created the next time the scene connects.
		// The scene may re-connect later, as its session was not neccessarily discarded (see UIApplicationDelegate `DidDiscardSceneSessions` instead).
	}

	public override void DidBecomeActive(UIScene scene)
	{
		// Called when the scene has moved from an inactive state to an active state.
		// Use this method to restart any tasks that were paused (or not yet started) when the scene was inactive.
	}

	public override void WillResignActive(UIScene scene)
	{
		// Called when the scene will move from an active state to an inactive state.
		// This may occur due to temporary interruptions (ex. an incoming phone call).
	}

	public override void WillEnterForeground(UIScene scene)
	{
		// Called as the scene transitions from the background to the foreground.
		// Use this method to undo the changes made on entering the background.
	}

	public override void DidEnterBackground(UIScene scene)
	{
		// Called as the scene transitions from the foreground to the background.
		// Use this method to save data, release shared resources, and store enough scene-specific state information
		// to restore the scene back to its current state.
	}
}
