using System.Collections.Specialized;
using SkiaSharp;
using Microsoft.Maui.Controls.Compatibility.Shapes;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.SkiaSharp
{
	public class PolygonRenderer : ShapeRenderer<Polygon, PolygonView>
	{
		public PolygonRenderer() : base()
		{
			RegisterPropertyHandler(Polygon.PointsProperty, UpdatePoints);
			RegisterPropertyHandler(Polygon.FillRuleProperty, UpdateFillRule);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Polygon> e)
		{
			if (Control == null)
			{
				SetNativeControl(new PolygonView());
			}

			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				var points = e.NewElement.Points;
				points.CollectionChanged += OnCollectionChanged;
			}
		}

		void UpdatePoints()
		{
			Control.UpdatePoints(Element.Points);
		}

		void UpdateFillRule()
		{
			Control.UpdateFillMode(Element.FillRule == FillRule.Nonzero);
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdatePoints();
		}
	}

	public class PolygonView : ShapeView
	{
		PointCollection _points;
		bool _fillMode;

		public PolygonView() : base()
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
				{
					path.LineTo(Forms.ConvertToScaledPixel(_points[index].X), Forms.ConvertToScaledPixel(_points[index].Y));
				}
				path.Close();

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
