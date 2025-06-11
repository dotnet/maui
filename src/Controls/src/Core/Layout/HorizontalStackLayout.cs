#nullable disable
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	[ContentProperty(nameof(Children))]
	public class HorizontalStackLayout : StackBase
	{
		protected override ILayoutManager CreateLayoutManager() => new HorizontalStackLayoutManager(this);

		internal override void ComputeConstraintForView(View view)
		{
			if ((Constraint & LayoutConstraint.VerticallyFixed) != 0 && view.VerticalOptions.Alignment == LayoutAlignment.Fill)
			{
				view.ComputedConstraint = LayoutConstraint.VerticallyFixed;
			}
			else
			{
				view.ComputedConstraint = LayoutConstraint.None;
			}
		}
	}
}
