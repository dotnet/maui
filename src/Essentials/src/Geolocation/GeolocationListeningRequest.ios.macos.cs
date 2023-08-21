// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace Microsoft.Maui.Devices.Sensors
{
	public partial class GeolocationListeningRequest
	{
		internal double PlatformDesiredAccuracy
		{
			get
			{
				return DesiredAccuracy.PlatformDesiredAccuracy();
			}
		}
	}
}
