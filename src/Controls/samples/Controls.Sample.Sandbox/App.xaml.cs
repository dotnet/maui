namespace Maui.Controls.Sample;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// The animation performance benchmark, when launched with ANIM_PERF=1 (environment variable or Android intent extra).
		if (PerfPage.IsRequested(activationState))
			return new Window(new PerfPage());

		// To test shell scenarios, change this to true
		bool useShell = false;

		if (useShell)
			return new Window(new SandboxShell());

		// The original functional demo of the animations prototype, when launched with ANIM_DEMO=classic.
		if (LaunchOptions.Get(activationState, "ANIM_DEMO") == "classic")
			return new Window(new NavigationPage(new MainPage()));

		// The festival player showcase. Its poster colors are designed for the light theme.
		UserAppTheme = AppTheme.Light;
		return new Window(new MainStagePage());
	}
}
