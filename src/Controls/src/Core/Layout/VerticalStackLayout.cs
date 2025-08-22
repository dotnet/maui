#nullable disable
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Children))]
	public class VerticalStackLayout : StackBase
	{
		protected override ILayoutManager CreateLayoutManager() => new VerticalStackLayoutManager(this);

		protected override SizeConstraint ComputeConstraintForView(View view)
		{
			if ((Constraint & SizeConstraint.HorizontallyFixed) != 0 && view.HorizontalOptions.Alignment == LayoutAlignment.Fill)
			{
				return SizeConstraint.HorizontallyFixed;
			}
			else
			{
				return SizeConstraint.None;
			}
		}
	}
}
