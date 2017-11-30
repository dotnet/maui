using System;
using Windows.UI.Xaml.Media;

namespace Xamarin.Forms.Platform.UWP
{
	internal static class BrushHelpers
	{
		/// <summary>
		/// Handles the logic for setting a Xamarin.Forms Color for a Brush
		/// while caching the original default brush
		/// </summary>
		/// <param name="color">The target Xamarin.Forms.Color</param>
		/// <param name="defaultbrush">The renderer's cache for the default brush</param>
		/// <param name="getter">Delegate for retrieving the Control's current Brush</param>
		/// <param name="setter">Delegate for setting the Control's Brush</param>
		public static void UpdateColor(Color color, ref Brush defaultbrush, Func<Brush> getter, Action<Brush> setter)
		{
			if (color.IsDefault)
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

			setter(color.ToBrush());
		}
	}
}