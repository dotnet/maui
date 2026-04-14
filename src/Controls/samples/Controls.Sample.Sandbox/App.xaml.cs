namespace Maui.Controls.Sample;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Keep shell enabled by default for shell-related sandbox scenarios.
		bool useShell = true;

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
