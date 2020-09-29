using System.ComponentModel;
using Android.Content;
using Xamarin.Forms.Shapes;
using static Android.Graphics.Path;
using APath = Android.Graphics.Path;

namespace Xamarin.Forms.Platform.Android
{
	public class PolylineRenderer : ShapeRenderer<Polyline, PolylineView>
	{
		public PolylineRenderer(Context context) : base(context)
		{

		}

		protected override void OnElementChanged(ElementChangedEventArgs<Polyline> args)
		{
			if (Control == null)
			{
				SetNativeControl(new PolylineView(Context));
			}

			base.OnElementChanged(args);

			if (args.NewElement != null)
			{
				UpdatePoints();
				UpdateFillRule();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
		{
			base.OnElementPropertyChanged(sender, args);

			if (args.PropertyName == Polyline.PointsProperty.PropertyName)
				UpdatePoints();
			else if (args.PropertyName == Polyline.FillRuleProperty.PropertyName)
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

		public PolylineView(Context context) : base(context)
		{
		}

		void UpdateShape()
		{
			if (_points != null && _points.Count > 1)
			{
				var path = new APath();
				path.SetFillType(_fillMode ? FillType.Winding : FillType.EvenOdd);

				path.MoveTo(_density * (float)_points[0].X, _density * (float)_points[0].Y);

				for (int index = 1; index < _points.Count; index++)
					path.LineTo(_density * (float)_points[index].X, _density * (float)_points[index].Y);

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