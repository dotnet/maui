namespace Microsoft.Maui
{
	public class ActivationState : IActivationState
	{
		public ActivationState(
			UI.Xaml.LaunchActivatedEventArgs launchActivatedEventArgs,
			UI.Xaml.Window mainWindow,
			IMauiContext context)
		{
			LaunchActivatedEventArgs = launchActivatedEventArgs;
			MainWindow = mainWindow;
			Context = context;
		}

		public UI.Xaml.LaunchActivatedEventArgs LaunchActivatedEventArgs { get; }

		public UI.Xaml.Window MainWindow { get; }

		public IMauiContext Context { get; }
	}
}