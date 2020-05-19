using Android.Content;
using System.Maui.PlatformConfiguration.AndroidSpecific;

namespace System.Maui.Platform.Android
{
	public static class ElevationHelper
	{
		public static void SetElevation(global::Android.Views.View view, VisualElement element)
		{
			if (view == null || element == null || !System.Maui.Maui.IsLollipopOrNewer)
			{
				return;
			}

			var elevation = GetElevation(element, view.Context);
			if (!elevation.HasValue)
			{
				return;
			}

			view.Elevation = elevation.Value;
		}

		internal static float? GetElevation(global::Android.Views.View view)
		{
			if (view == null || !System.Maui.Maui.IsLollipopOrNewer)
			{
				return null;
			}

			return view.Elevation;
		}

		internal static float? GetElevation(VisualElement element, Context context)
		{
			if (element == null || !System.Maui.Maui.IsLollipopOrNewer)
			{
				return null;
			}

			var iec = element as IElementConfiguration<VisualElement>;
			var elevation = iec?.On<PlatformConfiguration.Android>().GetElevation();

			if (elevation == null)
				return elevation;

			return context.ToPixels(elevation.Value);
		}
	}
}