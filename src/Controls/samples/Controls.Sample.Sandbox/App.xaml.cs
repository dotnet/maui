namespace Maui.Controls.Sample;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Test mode selection:
		// "shell"      - Test Shell handler migration
		// "tabbedpage" - Test TabbedPage with BottomNavigationManager
		// "navigation" - Test NavigationPage

		string testMode = "shell";

		//string testMode = "tabbedpage";

		//string testMode = "navigation";



		return testMode switch
		{
			"tabbedpage" => new Window(new TabbedPageTestPage()),
			"navigation" => new Window(new NavigationPage(new MainPage())),
			_ => new Window(new SandboxShell()) // default: shell
		};
	}
}
