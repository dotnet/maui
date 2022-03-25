#nullable enable

namespace Microsoft.Maui.Devices
{
	partial class DeviceDisplayImplementation : IDeviceDisplay
	{
		public bool KeepScreenOn { get => false; set { } }

		DisplayInfo GetMainDisplayInfo() => default;

		void StartScreenMetricsListeners() { }

		void StopScreenMetricsListeners() { }
	}
}
