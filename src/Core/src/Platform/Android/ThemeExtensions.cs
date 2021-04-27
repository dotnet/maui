using System;
using System.Collections.Generic;
using System.Text;
using Android.Content.Res;

namespace Microsoft.Maui
{
	public static class ThemeExtensions
	{
		public static bool TryResolveAttribute(this Resources.Theme? theme, int id)
		{
			return theme != null && theme.ResolveAttribute(id, new Android.Util.TypedValue(), resolveRefs: true);
		}

		public static bool TryResolveAttribute(this Resources.Theme? theme, int id, out bool? value)
		{
			using (var tv = new Android.Util.TypedValue())
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
	}
}
