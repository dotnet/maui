using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public static class KnownColor
	{
		public static Color Default => null;

		public static Color Transparent { get; } = new(255, 255, 255, 0);

		public static void SetAccent(Color value) => Accent = value;

		public static Color Accent { get; internal set; }
	}
}
