// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Controls.Maps
{
	public class PinClickedEventArgs : System.EventArgs
	{
		public bool HideInfoWindow { get; set; }

		public PinClickedEventArgs()
		{
			HideInfoWindow = false;
		}
	}
}
