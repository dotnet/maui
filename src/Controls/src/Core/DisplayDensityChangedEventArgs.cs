#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	public class DisplayDensityChangedEventArgs : EventArgs
	{
		public DisplayDensityChangedEventArgs(float displayDensity)
		{
			DisplayDensity = displayDensity;
		}

		public float DisplayDensity { get; }
	}
}