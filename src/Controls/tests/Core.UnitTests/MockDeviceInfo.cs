using System;
using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	class MockDeviceInfo : IDeviceInfo
	{
		public string Model { get; set; } = "Unit Test";

		public string Manufacturer { get; set; } = "Microsoft";

		public string Name { get; set; } = "Unit Test";

		public string VersionString { get; set; } = "1.0";

		public Version Version { get; set; } = new Version(1, 0);

		public DevicePlatform Platform { get; set; } = DevicePlatform.Create("UnitTest");

		public DeviceIdiom Idiom { get; set; } = DeviceIdiom.Phone;

		public DeviceType DeviceType { get; set; } = DeviceType.Virtual;
	}
}