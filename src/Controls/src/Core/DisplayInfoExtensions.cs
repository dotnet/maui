using System;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	static class DisplayInfoExtensions
	{
		public static Size GetScaledScreenSize(this DisplayInfo info) =>
			new Size(info.Width / info.Density, info.Height / info.Density);

		public static double DisplayRound(this DisplayInfo info, double value) =>
			Math.Round(value);
	}
}