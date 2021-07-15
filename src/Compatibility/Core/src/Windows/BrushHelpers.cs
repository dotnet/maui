using System;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	internal static class BrushHelpers
	{
		/// <summary>
		/// Handles the logic for setting a Microsoft.Maui.Controls.Compatibility Color for a Brush
		/// while caching the original default brush
		/// </summary>
		/// <param name="color">The target Microsoft.Maui.Controls.Compatibility.Color</param>
		/// <param name="defaultbrush">The renderer's cache for the default brush</param>
		/// <param name="getter">Delegate for retrieving the Control's current Brush</param>
		/// <param name="setter">Delegate for setting the Control's Brush</param>
		public static void UpdateColor(Color color, ref WBrush defaultbrush, Func<WBrush> getter, Action<WBrush> setter)
		{
			if (color.IsDefault())
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

			setter(Maui.ColorExtensions.ToNative(color));
		}

		public static void UpdateBrush(Brush brush, ref WBrush defaultbrush, Func<WBrush> getter, Action<WBrush> setter)
		{
			if (Brush.IsNullOrEmpty(brush))
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

			setter(brush.ToBrush());
		}
	}
}