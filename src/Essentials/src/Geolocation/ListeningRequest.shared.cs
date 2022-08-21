using System;

namespace Microsoft.Maui.Devices.Sensors
{
	public class ListeningRequest
	{
		public ListeningRequest()
			: this(GeolocationAccuracy.Default)
		{
		}

		public ListeningRequest(GeolocationAccuracy accuracy)
			:this(accuracy, TimeSpan.FromSeconds(1))
		{
		}

		public ListeningRequest(GeolocationAccuracy accuracy, TimeSpan minimumTime)
		{
			DesiredAccuracy = accuracy;
			MinimumTime = minimumTime;
		}

		public TimeSpan MinimumTime { get; set; } = TimeSpan.FromSeconds(1);

		public GeolocationAccuracy DesiredAccuracy { get; set; } = GeolocationAccuracy.Default;
	}
}
