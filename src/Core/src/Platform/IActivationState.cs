namespace Microsoft.Maui
{
	public interface IActivationState
	{
		public IMauiContext Context { get; }
#if __ANDROID__
		public Android.OS.Bundle? SavedInstance { get; }
#endif
	}
}
