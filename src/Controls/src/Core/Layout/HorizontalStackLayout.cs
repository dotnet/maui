#nullable disable
using Microsoft.Maui.Layouts;
using Microsoft.Maui.Primitives;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Children))]
	public class HorizontalStackLayout : StackBase
	{
		protected override ILayoutManager CreateLayoutManager() => new HorizontalStackLayoutManager(this);

		protected override SizeConstraint ComputeConstraintForView(View view)
		{
			if ((Constraint & SizeConstraint.VerticallyFixed) != 0 && view.VerticalOptions.Alignment == LayoutAlignment.Fill)
			{
				return SizeConstraint.VerticallyFixed;
			}
			else
			{
				return SizeConstraint.None;
			}
		}
	}
}
