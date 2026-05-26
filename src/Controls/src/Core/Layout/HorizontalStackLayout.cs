#nullable disable
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Children))]
	public class HorizontalStackLayout : StackBase
	{
		protected override ILayoutManager CreateLayoutManager() => new HorizontalStackLayoutManager(this);

		protected override LayoutConstraint ComputeConstraintForView(View view)
		{
			if ((Constraint & LayoutConstraint.VerticallyFixed) != 0 && view.VerticalOptions.Alignment == LayoutAlignment.Fill)
			{
				return LayoutConstraint.VerticallyFixed;
			}
			else
			{
				return LayoutConstraint.None;
			}
		}
	}
}
