using System;
using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class MockDeviceInfo : IDeviceInfo
	{
		public string Model { get; set; }

		public string Manufacturer { get; set; }

		public string Name { get; set; }

		public string VersionString { get; set; }

		public Version Version { get; set; }

		public DevicePlatform Platform { get; set; }

		public DeviceIdiom Idiom { get; set; }

		public DeviceType DeviceType { get; set; }
	}
}