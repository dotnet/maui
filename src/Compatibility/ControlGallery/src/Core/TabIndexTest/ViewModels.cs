using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.TabIndexTest
{
	public static class Colors
	{
		public static Color Cerulean = Color.FromRgb(0, 115, 209);
		public static Color Gray = Color.FromRgb(216, 221, 230);
		public static Color White = Color.White;
		public static Color Black = Color.Black;
	}
	[Flags]
	public enum DaysOfWeek
	{
		None = 0,
		Sunday = 1 << 0,
		Monday = 1 << 1,
		Tuesday = 1 << 2,
		Wednesday = 1 << 3,
		Thursday = 1 << 4,
		Friday = 1 << 5,
		Saturday = 1 << 6,
	}
}
