using AppKit;

namespace Maui.Controls.Sample.MacCatalyst;

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

		ConfigureMacWindowSize();
		ConfigureToolbar();
	}

	private void ConfigureMacWindowSize()
	{
		if (Window?.WindowScene?.SizeRestrictions is null)
			return;

		var fixedSize = new CGSize(400, 250);
		Window.WindowScene.SizeRestrictions.MinimumSize = fixedSize;
		Window.WindowScene.SizeRestrictions.MaximumSize = fixedSize;
	}

	private void ConfigureToolbar()
	{
		if (Window?.WindowScene?.Titlebar is null)
			return;

		var toolbar = new NSToolbar();
		
#pragma warning disable CA1422 
		toolbar.ShowsBaselineSeparator = false;
#pragma warning restore CA1422

		var titlebar = Window.WindowScene.Titlebar;
		titlebar.Toolbar = toolbar;
		titlebar.ToolbarStyle = UITitlebarToolbarStyle.Automatic;
		titlebar.TitleVisibility = UITitlebarTitleVisibility.Visible;
	}
}
