#nullable enable
using System;

namespace Microsoft.Maui.Essentials
{
	partial class DeviceDisplayImplementation : IDeviceDisplay
	{
		public event EventHandler<DisplayInfoChangedEventArgs>? MainDisplayInfoChanged
		{
			add { }
			remove { }
		}

		public bool KeepScreenOn { get => false; set { } }

		public DisplayInfo GetMainDisplayInfo() => default;

		public void StartScreenMetricsListeners() { }

		public void StopScreenMetricsListeners() { }
	}
}
