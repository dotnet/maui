namespace Microsoft.Maui
{
	public interface IActivationState
	{
		IMauiContext Context { get; }
#if __ANDROID__
		Android.OS.Bundle? SavedInstance { get; }
#elif IOS || MACCATALYST
		Foundation.NSUserActivity? UserActivity { get; }
#elif WINDOWS
		UI.Xaml.LaunchActivatedEventArgs? LaunchActivatedEventArgs { get; }
#endif
	}

	public interface ISaveableState
	{
		IMauiContext Context { get; }
#if __ANDROID__
		Android.OS.Bundle? Bundle { get; }
		Android.OS.PersistableBundle? PersistableBundle { get; }
#elif IOS || MACCATALYST
		Foundation.NSUserActivity? UserActivity { get; }
#elif WINDOWS
#endif
	}

	public interface IRestoredState
	{
		IMauiContext Context { get; }
#if __ANDROID__
		Android.OS.Bundle? Bundle { get; }
#elif IOS || MACCATALYST
		Foundation.NSUserActivity? UserActivity { get; }
#elif WINDOWS
#endif
	}
}
