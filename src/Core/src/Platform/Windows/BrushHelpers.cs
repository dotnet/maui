#nullable enable
using System;
using Microsoft.Maui.Graphics;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui
{
	internal static class BrushHelpers
	{
		public static void UpdateColor(Color color, ref WBrush? defaultbrush, Func<WBrush?> getter, Action<WBrush?> setter)
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

			setter(color.ToNative());
		}

		public static void UpdateBrush(Paint paint, ref WBrush? defaultbrush, Func<WBrush?> getter, Action<WBrush?> setter)
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

			setter(paint.ToNative());
		}
	}
}