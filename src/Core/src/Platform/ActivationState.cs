using System;

namespace Microsoft.Maui
{
	public class ActivationState : IActivationState
	{
#if __ANDROID__
		public ActivationState(IMauiContext context, Android.OS.Bundle? savedInstance)
			: this(context)
		{
			SavedInstance = savedInstance;
		}
#elif WINDOWS
		public ActivationState(IMauiContext context, UI.Xaml.LaunchActivatedEventArgs? launchActivatedEventArgs)
			: this(context)
		{
			LaunchActivatedEventArgs = launchActivatedEventArgs;
		}
#endif

		public ActivationState(IMauiContext context)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
		}

		public IMauiContext Context { get; }

#if __ANDROID__
		public Android.OS.Bundle? SavedInstance { get; }
#elif WINDOWS
		public UI.Xaml.LaunchActivatedEventArgs? LaunchActivatedEventArgs { get; }
#endif
	}
}