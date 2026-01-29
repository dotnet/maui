namespace Maui.Controls.Sample;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// To test Issue #33731 TabbedPage GC behavior
		// Change this to test different scenarios:
		// - true: Use TabbedPage to test GC monitoring
		// - false: Use regular MainPage
		bool useTabbedPage = true;

		if (useTabbedPage)
		{
			return new Window(new Issue33731TabbedPage());
		}
		
		// To test shell scenarios, change this to true
		bool useShell = false;

		if (!useShell)
		{
			return new Window(new NavigationPage(new MainPage()));
		}
		else
		{
			return new Window(new SandboxShell());
		}
	}
}
