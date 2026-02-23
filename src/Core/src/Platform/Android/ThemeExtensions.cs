using System;
using System.Collections.Generic;
using System.Text;
using Android.Content.Res;
using Android.Util;

namespace Microsoft.Maui.Platform
{
	public static class ThemeExtensions
	{
		public static bool TryResolveAttribute(this Resources.Theme? theme, int id)
		{
			return theme != null && theme.ResolveAttribute(id, new TypedValue(), resolveRefs: true);
		}

		public static bool TryResolveAttribute(this Resources.Theme? theme, int id, out bool? value)
		{
			using (var tv = new TypedValue())
			{
				if (theme != null && theme.ResolveAttribute(id, tv, resolveRefs: true))
				{
					if (tv.Data == 0)
						value = false;
					else
						value = true;

					return true;
				}
			}

			value = null;
			return false;
		}

		public static bool TryResolveAttribute(this Resources.Theme? theme, int id, out float? value)
		{
			using (var tv = new TypedValue())
			{
				if (theme != null && theme.ResolveAttribute(id, tv, resolveRefs: true))
				{
					value = tv.Data;
					return true;
				}
			}

			value = null;
			return false;
		}
	}
}
