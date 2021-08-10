using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class IndicatorView : ITemplatedIndicatorView
	{
		public Paint IndicatorsColor => IndicatorColor.AsPaint();

		public Paint PositionIndicatorColor => SelectedIndicatorColor.AsPaint();

		public IShapeView IndicatorShape => IndicatorsShape == Controls.IndicatorShape.Square ? new Shapes.Rectangle() : new Shapes.Ellipse();
		Maui.ILayout ITemplatedIndicatorView.IndicatorsLayoutOverride => (IndicatorTemplate != null) ? IndicatorLayout as Maui.ILayout : null;
	}
}
