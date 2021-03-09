using System.ComponentModel;
using SkiaSharp;
using Microsoft.Maui.Controls.Compatibility.Shapes;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.SkiaSharp
{
	public class PolylineRenderer : ShapeRenderer<Polyline, PolylineView>
	{
		public PolylineRenderer() : base()
		{
			RegisterPropertyHandler(Polyline.PointsProperty, UpdatePoints);
			RegisterPropertyHandler(Polyline.FillRuleProperty, UpdateFillRule);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Polyline> e)
		{
			if (Control == null)
			{
				SetNativeControl(new PolylineView());
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Polyline.PointsProperty.PropertyName)
				UpdatePoints();
			else if (e.PropertyName == Polyline.FillRuleProperty.PropertyName)
				UpdateFillRule();
		}

		void UpdatePoints()
		{
			Control.UpdatePoints(Element.Points);
		}

		void UpdateFillRule()
		{
			Control.UpdateFillMode(Element.FillRule == FillRule.Nonzero);
		}
	}

	public class PolylineView : ShapeView
	{
		PointCollection _points;
		bool _fillMode;

		public PolylineView() : base()
		{
		}

		void UpdateShape()
		{
			if (_points != null && _points.Count > 1)
			{
				SKPath path = new SKPath();
				path.FillType = _fillMode ? SKPathFillType.Winding : SKPathFillType.EvenOdd;

				path.MoveTo(Forms.ConvertToScaledPixel(_points[0].X), Forms.ConvertToScaledPixel(_points[0].Y));

				for (int index = 1; index < _points.Count; index++)
					path.LineTo(Forms.ConvertToScaledPixel(_points[index].X), Forms.ConvertToScaledPixel(_points[index].Y));

				UpdateShape(path);
			}
		}

		public void UpdatePoints(PointCollection points)
		{
			_points = points;
			UpdateShape();
		}

		public void UpdateFillMode(bool fillMode)
		{
			_fillMode = fillMode;
			UpdateShape();
		}
	}
}
