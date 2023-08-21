// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Controls.Maps
{
	public class MapClickedEventArgs
	{
		public Location Location { get; }

		public MapClickedEventArgs(Location location)
		{
			Location = location;
		}
	}
}
