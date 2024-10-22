namespace Maui.Controls.Sample;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	Page? page = null;

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// To test shell scenarios, change this to true
		bool useShell = false;

		if (!useShell)
		{
			var previousWindow = page?.Window;
			var newWindow = new Window(page ??= new NavigationPage(new MainPage()));

			var pages = previousWindow?.Page;

			return newWindow;
		}
		else
		{
			return new Window(new SandboxShell());
		}
	}
}
