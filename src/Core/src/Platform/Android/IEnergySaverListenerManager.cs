// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Platform
{
	public interface IEnergySaverListenerManager
	{
		void Add(IEnergySaverListener listener);

		void Remove(IEnergySaverListener listener);
	}
}