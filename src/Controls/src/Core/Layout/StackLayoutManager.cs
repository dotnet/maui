#nullable disable
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public class StackLayoutManager : ILayoutManager
	{
		readonly StackLayout _stackLayout;

		public StackLayoutManager(StackLayout stackLayout)
		{
			_stackLayout = stackLayout;
		}

		VerticalStackLayoutManager _verticalStackLayoutManager;
		HorizontalStackLayoutManager _horizontalStackLayoutManager;
		AndExpandLayoutManager _andExpandLayoutManager;

		ILayoutManager SelectLayoutManager()
		{
			if (UsesExpansion(_stackLayout))
			{
				return _andExpandLayoutManager ??= new AndExpandLayoutManager(_stackLayout);
			}

			if (_stackLayout.Orientation == StackOrientation.Vertical)
			{
				return _verticalStackLayoutManager ??= new VerticalStackLayoutManager(_stackLayout);
			}

			return _horizontalStackLayoutManager ??= new HorizontalStackLayoutManager(_stackLayout);
		}

		public Size ArrangeChildren(Rect bounds)
		{
			return SelectLayoutManager().ArrangeChildren(bounds);
		}

		public Size Measure(double widthConstraint, double heightConstraint)
		{
			return SelectLayoutManager().Measure(widthConstraint, heightConstraint);
		}

		static bool UsesExpansion(StackLayout stackLayout)
		{
			var orientation = stackLayout.Orientation;

			for (int n = 0; n < stackLayout.Count; n++)
			{
				if (stackLayout[n] is View view)
				{
					// Validate the expansion direction against the orientation of the StackLayout
					// Horizontal expansion in a vertical stack layout makes no sense, so we ignore it
					// Same with vertical expansion in a horizontal layout

					if (orientation == StackOrientation.Vertical && view.VerticalOptions.Expands)
					{
						return true;
					}

					if (orientation == StackOrientation.Horizontal && view.HorizontalOptions.Expands)
					{
						return true;
					}
				}
			}

			return false;
		}
	}
}
