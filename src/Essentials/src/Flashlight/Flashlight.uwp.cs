using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Lights;

namespace Microsoft.Maui.Essentials
{
	public static partial class Flashlight
	{
		static readonly object locker = new object();
		static bool hasLoadedLamp;
		static Lamp lamp;

		static async Task FindLampAsync()
		{
			// fail fast
			if (hasLoadedLamp)
				return;

			Monitor.Enter(locker);

			// we may have loaded it while this was waiting to enter
			if (hasLoadedLamp)
				return;

			// find all the lamps
			var selector = Lamp.GetDeviceSelector();
			var allLamps = await DeviceInformation.FindAllAsync(selector);

			// find all the back lamps
			var lampInfo = allLamps.FirstOrDefault(di => di.EnclosureLocation?.Panel == Panel.Back);

			if (lampInfo != null)
			{
				// get the lamp
				lamp = await Lamp.FromIdAsync(lampInfo.Id);
			}
			else
			{
				// if there is no back lamp, use the default lamp
				lamp = await Lamp.GetDefaultAsync();
			}

			hasLoadedLamp = true;
			Monitor.Exit(locker);
		}

		static async Task PlatformTurnOnAsync()
		{
			await FindLampAsync();

			if (lamp == null)
				throw new FeatureNotSupportedException();

			lock (locker)
			{
				if (lamp != null)
				{
					lamp.BrightnessLevel = 1.0f;
					lamp.IsEnabled = true;
				}
			}
		}

		static Task PlatformTurnOffAsync()
		{
			lock (locker)
			{
				if (lamp != null)
				{
					lamp.IsEnabled = false;
					lamp.Dispose();
					lamp = null;
					hasLoadedLamp = false;
				}
			}

			return Task.CompletedTask;
		}
	}
}
