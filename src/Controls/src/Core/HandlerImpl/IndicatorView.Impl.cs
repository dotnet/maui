using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.Controls
{
	public partial class IndicatorView : ITemplatedIndicatorView
	{
		public Paint IndicatorsColor => IndicatorColor.AsPaint();

		public Paint PositionIndicatorColor => SelectedIndicatorColor.AsPaint();

		public IShapeView IndicatorShape
		{
			get
			{
				return IndicatorsShape == Controls.IndicatorShape.Square ? new Shapes.Rectangle() { WidthRequest = IndicatorSize, HeightRequest = IndicatorSize,
																										} : 
																		   new Shapes.Ellipse() { WidthRequest = IndicatorSize, HeightRequest = IndicatorSize,
																			   Fill = Brush.Red,
																			   Stroke = Brush.Red,
																			   Aspect = Stretch.Uniform,
																			   HorizontalOptions = LayoutOptions.Center,
																			   VerticalOptions = LayoutOptions.Center,
																		   };
			}
		}

		Maui.ILayout ITemplatedIndicatorView.IndicatorsLayoutOverride => (IndicatorTemplate != null) ? IndicatorLayout as Maui.ILayout : null;
	}
}
