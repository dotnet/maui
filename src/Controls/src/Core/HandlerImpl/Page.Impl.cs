using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class Page : IPage
	{
		IView IPage.Content => null;

		Size IArrangeable.Arrange(Rectangle bounds)
		{
			return ArrangeOverride(bounds);
		}

		Size IArrangeable.Measure(double widthConstraint, double heightConstraint)
		{
			return MeasureOverride(widthConstraint, heightConstraint);
		}
	}
}
