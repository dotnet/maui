namespace Maui.Controls.Sample;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// To test shell scenarios, change this to true
		bool useShell = false;

		if (!useShell)
		{
			return new Window(new TopPage());
		}
		else
		{
			return new Window(new SandboxShell());
		}
	}
}
