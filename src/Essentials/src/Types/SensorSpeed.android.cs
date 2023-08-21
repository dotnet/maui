// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Android.Hardware;

namespace Microsoft.Maui.Devices.Sensors
{
	internal static partial class SensorSpeedExtensions
	{
		internal static SensorDelay ToPlatform(this SensorSpeed sensorSpeed)
		{
			switch (sensorSpeed)
			{
				case SensorSpeed.Fastest:
					return SensorDelay.Fastest;
				case SensorSpeed.Game:
					return SensorDelay.Game;
				case SensorSpeed.UI:
					return SensorDelay.Ui;
			}

			return SensorDelay.Normal;
		}
	}
}
