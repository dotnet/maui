using System;
using System.Linq;

namespace Microsoft.Maui.Handlers
{
	internal static class LayoutExtensions
	{
		public static IOrderedEnumerable<IView> OrderByZIndex(this ILayout layout) => layout.OrderBy(v => v.ZIndex);

		public static int GetLayoutHandlerIndex(this ILayout layout, IView view)
		{
			bool found = false;
			var lowerViews = GetLowerViewsCount(layout, view, ref found);
			return found ? lowerViews : -1;
		}

		static int GetLowerViewsCount(ILayout layout, IView view, ref bool found)
		{
			var count = layout.Count;
			switch (count)
			{
				case 0:
					return 0;
				case 1:
					if (view == layout[0])
					{
						found = true;
						return 0;
					}
					return 0;
				default:
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
#if IOS || ANDROID || MACCATALYST
							lowerViews += child is ICompressedLayout { IsHeadless: true } and ILayout headlessLayout
								? GetLowerViewsCount(headlessLayout, view, ref found)
								: 1;
#else
							++lowerViews;
#endif
						}
					}

					return lowerViews;
			}
		}
	}
}
