using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Handlers
{
	internal static class LayoutExtensions
	{
		class ZIndexComparer : IComparer<IView>
		{
			public int Compare(IView? x, IView? y)
			{
				if (x == null || y == null)
				{
					return 0;
				}

				return x.ZIndex.CompareTo(y.ZIndex);
			}
		}

		static ZIndexComparer s_comparer = new();

		public static IView[] OrderByZIndex(this ILayout layout)
		{
			var ordered = new IView[layout.Count];
			layout.CopyTo(ordered, 0);
			Array.Sort(ordered, s_comparer);
			return ordered;
		}

		public static int GetLayoutHandlerIndex(this ILayout layout, IView view)
		{
			return layout.OrderByZIndex().IndexOf(view);
		}
	}
}
