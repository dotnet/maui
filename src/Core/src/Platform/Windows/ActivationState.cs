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

		public Microsoft.UI.Xaml.LaunchActivatedEventArgs LaunchActivatedEventArgs { get; }
		
		public global::Microsoft.UI.Xaml.Window MainWindow { get; }
		public IMauiContext Context { get; }
	}
}