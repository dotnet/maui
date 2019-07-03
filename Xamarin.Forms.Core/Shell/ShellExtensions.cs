using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.Core
{
	internal static class ShellExtensions
	{
		public static T SearchForRoute<T>(this Shell shell, string route) where T : BaseShellItem =>
			(T)SearchForRoute(shell, route);

		public static BaseShellItem SearchForRoute(this Shell shell, string route) =>
			SearchForPart(shell, (p) => p.Route == route);


		public static T SearchForRoute<T>(this BaseShellItem shell, string route) where T : BaseShellItem =>
			(T)SearchForRoute(shell, route);

		public static BaseShellItem SearchForRoute(this BaseShellItem shell, string route) =>
			SearchForPart(shell, (p) => p.Route == route);


		public static BaseShellItem SearchForPart(this Shell shell, Func<BaseShellItem, bool> searchBy)
		{
			for (var i = 0; i < shell.Items.Count; i++)
			{
				var result = SearchForPart(shell.Items[i], searchBy);
				if (result != null)
					return result;
			}

			return null;
		}

		public static BaseShellItem SearchForPart(this BaseShellItem part, Func<BaseShellItem, bool> searchBy)
		{
			if (searchBy(part))
				return part;

			BaseShellItem baseShellItem = null;
			switch (part)
			{
				case ShellItem item:
					foreach (var section in item.Items)
					{
						baseShellItem = SearchForPart(section, searchBy);
						if (baseShellItem != null)
							return baseShellItem;
					}
					break;
				case ShellSection section:
					foreach (var content in section.Items)
					{
						baseShellItem = SearchForPart(content, searchBy);
						if (baseShellItem != null)
							return baseShellItem;
					}
					break;
			}

			return null;
		}
	}
}
