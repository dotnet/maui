using System.Linq;
using SkiaSharp;
using Xamarin.Forms.Shapes;
using Shape = Xamarin.Forms.Shapes.Shape;

namespace Xamarin.Forms.Platform.Tizen.SkiaSharp
{
	public class ShapeRenderer<TShape, TNativeShape> : ViewRenderer<TShape, TNativeShape>
		where TShape : Shape
		where TNativeShape : ShapeView
	{

		public ShapeRenderer()
		{
			RegisterPropertyHandler(Shape.AspectProperty, UpdateAspect);
			RegisterPropertyHandler(Shape.FillProperty, UpdateFill);
			RegisterPropertyHandler(Shape.StrokeProperty, UpdateStroke);
			RegisterPropertyHandler(Shape.StrokeThicknessProperty, UpdateStrokeThickness);
			RegisterPropertyHandler(Shape.StrokeDashArrayProperty, UpdateStrokeDashArray);
			RegisterPropertyHandler(Shape.StrokeDashOffsetProperty, UpdateStrokeDashOffset);
			RegisterPropertyHandler(Shape.StrokeLineCapProperty, UpdateStrokeLineCap);
			RegisterPropertyHandler(Shape.StrokeLineJoinProperty, UpdateStrokeLineJoin);
			RegisterPropertyHandler(Shape.StrokeMiterLimitProperty, UpdateStrokeMiterLimit);
		}

		void UpdateAspect()
		{
			Control.UpdateAspect(Element.Aspect);
		}

		void UpdateFill()
		{
			Control.UpdateFill(Element.Fill);
		}

		void UpdateStroke()
		{
			Control.UpdateStroke(Element.Stroke);
		}

		void UpdateStrokeThickness()
		{
			Control.UpdateStrokeThickness(Element.StrokeThickness);
		}

		void UpdateStrokeDashArray()
		{
			Control.UpdateStrokeDashArray(Element.StrokeDashArray.Select(x => (float)x).ToArray());
		}

		void UpdateStrokeDashOffset()
		{
			Control.UpdateStrokeDashOffset((float)Element.StrokeDashOffset);
		}

		void UpdateStrokeLineCap()
		{
			PenLineCap lineCap = Element.StrokeLineCap;
			SKStrokeCap skStrokeCap = SKStrokeCap.Butt;
			switch (lineCap)
			{
				case PenLineCap.Flat:
					skStrokeCap = SKStrokeCap.Butt;
					break;
				case PenLineCap.Square:
					skStrokeCap = SKStrokeCap.Square;
					break;
				case PenLineCap.Round:
					skStrokeCap = SKStrokeCap.Round;
					break;
			}
			Control.UpdateStrokeLineCap(skStrokeCap);
		}

		void UpdateStrokeLineJoin()
		{
			PenLineJoin lineJoin = Element.StrokeLineJoin;
			SKStrokeJoin skStrokeJoin = SKStrokeJoin.Miter;
			switch (lineJoin)
			{
				case PenLineJoin.Miter:
					skStrokeJoin = SKStrokeJoin.Miter;
					break;
				case PenLineJoin.Bevel:
					skStrokeJoin = SKStrokeJoin.Bevel;
					break;
				case PenLineJoin.Round:
					skStrokeJoin = SKStrokeJoin.Round;
					break;
			}
			Control.UpdateStrokeLineJoin(skStrokeJoin);
		}

		void UpdateStrokeMiterLimit()
		{
			Control.UpdateStrokeMiterLimit((float)Element.StrokeMiterLimit);
		}
	}
}
