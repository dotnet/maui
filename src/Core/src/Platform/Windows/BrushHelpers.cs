#nullable enable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Platform
{
	internal static class BrushHelpers
	{
		public static void UpdateColor(Color? color, ref Brush? defaultbrush, Func<Brush?> getter, Action<Brush?> setter)
		{
			if (color == null)
			{
				if (defaultbrush == null)
				{
					return;
				}

				setter(defaultbrush);
				return;
			}

			if (defaultbrush == null)
			{
				defaultbrush = getter();
			}

			setter(color.ToPlatform());
		}

		public static void UpdateBrush(Paint paint, ref Brush? defaultbrush, Func<Brush?> getter, Action<Brush?> setter)
		{
			if (paint == null)
			{
				if (defaultbrush == null)
				{
					return;
				}

				setter(defaultbrush);
				return;
			}

			if (defaultbrush == null)
			{
				defaultbrush = getter();
			}

			setter(paint.ToPlatform());
		}
	}
}