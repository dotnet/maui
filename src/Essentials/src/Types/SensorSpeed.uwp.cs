// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Devices.Sensors
{
	internal static partial class SensorSpeedExtensions
	{
		internal static uint ToPlatform(this SensorSpeed sensorSpeed)
		{
			switch (sensorSpeed)
			{
				case SensorSpeed.Fastest:
					return sensorIntervalFastest;
				case SensorSpeed.Game:
					return sensorIntervalGame;
				case SensorSpeed.UI:
					return sensorIntervalUI;
			}

			return sensorIntervalDefault;
		}
	}
}
