namespace Microsoft.Maui
{
	public class ActivationState : IActivationState
	{
		public ActivationState(
			IMauiContext context,
			UI.Xaml.LaunchActivatedEventArgs? launchActivatedEventArgs = null)
		{
			Context = context;
			LaunchActivatedEventArgs = launchActivatedEventArgs;
		}

		public IMauiContext Context { get; }

		public UI.Xaml.LaunchActivatedEventArgs? LaunchActivatedEventArgs { get; }
	}

	public class RestoredState : IRestoredState
	{
		public RestoredState(IMauiContext context)
		{
			Context = context;
		}

		public IMauiContext Context { get; }
	}

	public class SaveableState : ISaveableState
	{
		public SaveableState(IMauiContext context)
		{
			Context = context;
		}

		public IMauiContext Context { get; }
	}
}