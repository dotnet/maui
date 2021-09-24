namespace Microsoft.Maui.Platform
{
	public interface IEnergySaverListenerManager
	{
		void Add(IEnergySaverListener listener);

		void Remove(IEnergySaverListener listener);
	}
}