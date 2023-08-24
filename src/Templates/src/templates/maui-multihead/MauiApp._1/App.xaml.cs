namespace MauiApp._1;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		if (activationState is null)
			throw new InvalidProgramException("Unable to create a window as there is no activation state.");

		var services = activationState.Context.Services;
		var window = services.GetRequiredService<AppWindow>();
		return window;
	}
}
