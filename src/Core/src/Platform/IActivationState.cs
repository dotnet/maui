namespace Microsoft.Maui
{
	public interface IActivationState
	{
		IMauiContext Context { get; }
#if __ANDROID__
		Android.OS.Bundle? SavedInstance { get; }
#elif WINDOWS
		UI.Xaml.LaunchActivatedEventArgs LaunchActivatedEventArgs { get; }
#endif
	}
}
