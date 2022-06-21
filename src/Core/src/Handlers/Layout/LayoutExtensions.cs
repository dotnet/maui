using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Handlers
{
	internal static class LayoutExtensions
	{
		record ViewAndIndex(IView View, int Index);

		class ZIndexComparer : IComparer<ViewAndIndex>
		{
			public int Compare(ViewAndIndex? x, ViewAndIndex? y)
			{
				if (x == null || y == null)
				{
					return 0;
				}

				var zIndexCompare = x.View.ZIndex.CompareTo(y.View.ZIndex);

				if (zIndexCompare == 0)
				{
					return x.Index.CompareTo(y.Index);
				}

				return zIndexCompare;
			}
		}

		static ZIndexComparer s_comparer = new();

		public static IView[] OrderByZIndex(this ILayout layout)
		{
			var count = layout.Count;
			var indexedViews = new ViewAndIndex[count];

			for (int n = 0; n < count; n++)
			{
				indexedViews[n] = new ViewAndIndex(layout[n], n);
			}

			Array.Sort(indexedViews, s_comparer);

			var ordered = new IView[count];

			for (int n = 0; n < count; n++)
			{
				ordered[n] = indexedViews[n].View;
			}

			return ordered;
		}

		public static int GetLayoutHandlerIndex(this ILayout layout, IView view)
		{
			return layout.OrderByZIndex().IndexOf(view);
		}
	}
}
