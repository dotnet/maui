// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Devices.Sensors;

namespace Microsoft.Maui.Maps
{
	public interface ICircleMapElement : IMapElement, IFilledMapElement
	{
		Location Center { get; }
		Distance Radius { get; }
	}
}
