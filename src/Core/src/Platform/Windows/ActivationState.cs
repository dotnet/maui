namespace Microsoft.Maui
{
	public class ActivationState : IActivationState
	{
		public ActivationState(
			IMauiContext context,
			UI.Xaml.LaunchActivatedEventArgs? launchActivatedEventArgs = null,
			IWindow? newWindow = null)
		{
			Context = context;
			LaunchActivatedEventArgs = launchActivatedEventArgs;
			NewWindow = newWindow;
		}

		public IMauiContext Context { get; }

		public UI.Xaml.LaunchActivatedEventArgs? LaunchActivatedEventArgs { get; }

		public IWindow? NewWindow { get; }
	}
}