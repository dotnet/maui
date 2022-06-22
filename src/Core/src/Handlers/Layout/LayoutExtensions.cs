using System;
using System.Linq;

namespace Microsoft.Maui.Handlers
{
	internal static class LayoutExtensions
	{
		public static IOrderedEnumerable<IView> OrderByZIndex(this ILayout layout) => layout.OrderBy(v => v.ZIndex);

		public static int GetLayoutHandlerIndex(this ILayout layout, IView view)
		{
			switch (layout.Count)
			{
				case 0:
					return -1;
				case 1:
					return view == layout[0] ? 0 : -1;
				default:
					return layout.OrderByZIndex().IndexOf(view);
			}
		}
	}
}
