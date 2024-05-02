using System;
using System.Linq;

namespace Microsoft.Maui.Handlers
{
	internal static class LayoutExtensions
	{
		public static IOrderedEnumerable<IView> OrderByZIndex(this ILayout layout) => layout.OrderBy(v => v.ZIndex);

		public static int GetLayoutHandlerIndex(this ILayout layout, IView view)
		{
			var count = layout.Count;
			switch (count)
			{
				case 0:
					return -1;
				case 1:
					return view == layout[0] ? 0 : -1;
				default:
					var found = false;
					var zIndex = view.ZIndex;
					var lowerViews = 0;

					for (int i = 0; i < count; i++)
					{
						var child = layout[i];
						var childZIndex = child.ZIndex;

						if (child == view)
						{
							found = true;
						}

						if (childZIndex < zIndex || !found && childZIndex == zIndex)
						{
							++lowerViews;
						}
					}

					return found ? lowerViews : -1;
			}
		}
	}
}
