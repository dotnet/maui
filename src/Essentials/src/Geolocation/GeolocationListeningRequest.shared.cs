using System;

namespace Microsoft.Maui.Devices.Sensors
{
	public partial class GeolocationListeningRequest
	{
		public GeolocationListeningRequest()
			: this(GeolocationAccuracy.Default)
		{
		}

		public GeolocationListeningRequest(GeolocationAccuracy accuracy)
			:this(accuracy, TimeSpan.FromSeconds(1))
		{
		}

		public GeolocationListeningRequest(GeolocationAccuracy accuracy, TimeSpan minimumTime)
		{
			DesiredAccuracy = accuracy;
			MinimumTime = minimumTime;
		}

		public TimeSpan MinimumTime { get; set; } = TimeSpan.FromSeconds(1);

		public GeolocationAccuracy DesiredAccuracy { get; set; } = GeolocationAccuracy.Default;
	}
}
