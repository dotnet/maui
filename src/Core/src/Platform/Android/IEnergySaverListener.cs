namespace Microsoft.Maui.Platform
{
	public interface IEnergySaverListener
	{
		void OnStatusUpdated(bool energySaverEnabled);
	}
}