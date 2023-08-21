// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public class BackgroundingEventArgs : EventArgs
	{
		public BackgroundingEventArgs(IPersistedState state)
		{
			State = state;
		}

		public IPersistedState State { get; set; }
	}
}