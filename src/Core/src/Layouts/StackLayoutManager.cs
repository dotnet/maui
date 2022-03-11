using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Layouts
{
	public abstract class StackLayoutManager : LayoutManager
	{
		public StackLayoutManager(IStackLayout stack) : base(stack)
		{
			Stack = stack;
		}

		public IStackLayout Stack { get; }

		protected static double MeasureSpacing(double spacing, int childCount)
		{
			return childCount > 1 ? (childCount - 1) * spacing : 0;
		}
	}
}
