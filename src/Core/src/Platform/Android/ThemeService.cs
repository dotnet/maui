using System;
using System.Collections.Generic;
using System.Text;
using Android.Content.Res;

namespace Microsoft.Maui.Platform
{
	internal static class ThemeService
	{
		public static event Action<UiMode>? ThemeChanged;

		public static void NotifyThemeChange(UiMode uiMode)
		{
			ThemeChanged?.Invoke(uiMode);
		}
	}
}
