namespace Microsoft.Maui
{
	public interface IActivationState
	{
		IMauiContext Context { get; }

		IPersistedState State { get; }
	}
}