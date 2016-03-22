using System.Collections.Generic;

namespace Xamarin.Forms.UITest.TestCloud
{
	internal class DeviceSet
	{
		public DeviceSet(List<Platform> deviceSetPlatform, string id, List<string> devices)
		{
			Id = id;
			DeviceSetPlatform = deviceSetPlatform;
			Devices = devices;
		}

		public string Id { get; private set; }

		public List<Platform> DeviceSetPlatform { get; private set; }

		public List<string> Devices { get; private set; }

		internal enum Platform
		{
			None,
			Android,
			IOs,
			IOsClassic
		}
	}
}