using System;

namespace Microsoft.Maui.Controls
{
	public class DisplayDensityUpdatedEventArgs : EventArgs
	{
		public DisplayDensityUpdatedEventArgs(float displayDensity)
		{
			DisplayDensity = displayDensity;
		}

		public float DisplayDensity { get; }
	}
}