#nullable disable
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Controls.Shapes
{
	public static class EllipseExtensions
	{
		public static bool TrySetAppTheme(
			this Ellipse ellipse,
			string lightResourceKey,
			string darkResourceKey,
			BindableProperty bindableProperty,
			Brush defaultDark,
			Brush defaultLight,
			out object outerLight,
			out object outerDark)
		{
			bool anyValueSet = false;
			if (!Application.Current.TryGetResource(lightResourceKey, out outerLight))
			{
				anyValueSet = true;
				outerLight = defaultLight;
			}
			if (!Application.Current.TryGetResource(darkResourceKey, out outerDark))
			{
				anyValueSet = true;
				outerDark = defaultDark;
			}

			ellipse.SetAppTheme(bindableProperty, outerLight, outerDark);
			return anyValueSet;
		}

		public static bool TrySetDynamicThemeColor(
			this Ellipse ellipse,
			string resourceKey,
			BindableProperty bindableProperty,
			out object outerColor)
		{
			if (Application.Current.TryGetResource(resourceKey, out outerColor))
			{
				ellipse.SetDynamicResource(bindableProperty, resourceKey);
				return true;
			}

			return false;
		}

		public static void TrySetFillFromAppTheme(
			this Ellipse ellipse,
			string lightResourceKey,
			string darkResourceKey,
			Brush defaultDark,
			Brush defaultLight)
		{
			if (!Application.Current.TryGetResource(lightResourceKey, out var outerLight))
			{
				outerLight = defaultLight;
			}
			if (!Application.Current.TryGetResource(darkResourceKey, out var outerDark))
			{
				outerDark = defaultDark;
			}

			ellipse.Fill = (Brush)(Application.Current?.RequestedTheme == AppTheme.Dark ? outerDark : outerLight);
		}
	}
}

