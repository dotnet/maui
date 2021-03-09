using SkiaSharp;
using Microsoft.Maui.Controls.Compatibility.Shapes;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen.SkiaSharp
{
	public class LineRenderer : ShapeRenderer<Line, LineView>
	{
		public LineRenderer() : base()
		{
			RegisterPropertyHandler(Line.X1Property, UpdateX1);
			RegisterPropertyHandler(Line.Y1Property, UpdateY1);
			RegisterPropertyHandler(Line.X2Property, UpdateX2);
			RegisterPropertyHandler(Line.Y2Property, UpdateY2);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Line> e)
		{
			if (Control == null)
			{
				SetNativeControl(new LineView());
			}

			base.OnElementChanged(e);
		}

		void UpdateX1()
		{
			Control.UpdateX1((float)Element.X1);
		}

		void UpdateY1()
		{
			Control.UpdateY1((float)Element.Y1);
		}

		void UpdateX2()
		{
			Control.UpdateX2((float)Element.X2);
		}

		void UpdateY2()
		{
			Control.UpdateY2((float)Element.Y2);
		}
	}

	public class LineView : ShapeView
	{
		float _x1, _y1, _x2, _y2;

		public LineView() : base()
		{
		}

		void UpdateShape()
		{
			var path = new SKPath();
			path.MoveTo(_x1, _y1);
			path.LineTo(_x2, _y2);
			UpdateShape(path);
		}

		public void UpdateX1(float x1)
		{
			_x1 = Forms.ConvertToScaledPixel(x1);
			UpdateShape();
		}

		public void UpdateY1(float y1)
		{
			_y1 = Forms.ConvertToScaledPixel(y1);
			UpdateShape();
		}

		public void UpdateX2(float x2)
		{
			_x2 = Forms.ConvertToScaledPixel(x2);
			UpdateShape();
		}

		public void UpdateY2(float y2)
		{
			_y2 = Forms.ConvertToScaledPixel(y2);
			UpdateShape();
		}
	}
}
