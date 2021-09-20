using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class IndicatorView : ITemplatedIndicatorView
	{
		Paint IIndicatorView.IndicatorColor => IndicatorColor?.AsPaint();
		Paint IIndicatorView.SelectedIndicatorColor => SelectedIndicatorColor?.AsPaint();
		IShape IIndicatorView.IndicatorsShape => IndicatorsShape == IndicatorShape.Square ? new Shapes.Rectangle() : new Shapes.Ellipse();
		Maui.ILayout ITemplatedIndicatorView.IndicatorsLayoutOverride => (IndicatorTemplate != null) ? IndicatorLayout as Maui.ILayout : null;
	}
}
