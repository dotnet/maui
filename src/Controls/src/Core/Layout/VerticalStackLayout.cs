#nullable disable
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Children))]
	public class VerticalStackLayout : StackBase
	{
		protected override ILayoutManager CreateLayoutManager() => new VerticalStackLayoutManager(this);

		protected override LayoutConstraint ComputeConstraintForView(View view)
		{
			if ((Constraint & LayoutConstraint.HorizontallyFixed) != 0 && view.HorizontalOptions.Alignment == LayoutAlignment.Fill)
			{
				return LayoutConstraint.HorizontallyFixed;
			}
			else
			{
				return LayoutConstraint.None;
			}
		}
	}
}
