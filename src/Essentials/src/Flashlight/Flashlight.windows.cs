using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Devices.Lights;

namespace Microsoft.Maui.Devices
{
	class FlashlightImplementation : IFlashlight
	{
		static readonly object locker = new object();
		bool hasLoadedLamp;
		Lamp lamp;

		async Task FindLampAsync()
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

			if (lampInfo is not null)
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

		/// <summary>
		/// Checks if the flashlight is available and can be turned on or off.
		/// </summary>
		/// <returns><see langword="true"/> when the flashlight is available, or <see langword="false"/> when not</returns>
		public async Task<bool> IsSupportedAsync()
		{
			await FindLampAsync();

			lock (locker)
			{
				return hasLoadedLamp && lamp is not null;
			}
		}

		public async Task TurnOnAsync()
		{
			await FindLampAsync();

			if (lamp == null)
				throw new FeatureNotSupportedException();

			lock (locker)
			{
				if (lamp is not null)
				{
					lamp.BrightnessLevel = 1.0f;
					lamp.IsEnabled = true;
				}
			}
		}

		public Task TurnOffAsync()
		{
			lock (locker)
			{
				if (lamp is not null)
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
