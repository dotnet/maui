using System.Collections.Generic;

namespace Microsoft.Maui
{
	public class ActivationState : IActivationState
	{
		public ActivationState(
			IMauiContext context,
			UI.Xaml.LaunchActivatedEventArgs? launchActivatedEventArgs = null)
		{
			Context = context;
			State = new Dictionary<string, string?>();
			LaunchActivatedEventArgs = launchActivatedEventArgs;
		}

		public IMauiContext Context { get; }

		public IReadOnlyDictionary<string, string?> State { get; }

		public UI.Xaml.LaunchActivatedEventArgs? LaunchActivatedEventArgs { get; }
	}
}