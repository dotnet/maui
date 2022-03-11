namespace Microsoft.Maui.Handlers
{
#if WINDOWS
	public record OpenWindowRequest(IPersistedState? State = null, UI.Xaml.LaunchActivatedEventArgs? LaunchArgs = null);
#else
	public record OpenWindowRequest(IPersistedState? State = null);
#endif
}