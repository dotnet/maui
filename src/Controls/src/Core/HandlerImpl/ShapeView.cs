#nullable enable
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes2
{
	public class ShapeView : View, IShapeView
	{
		public ShapeView()
		{

		}

		public ShapeView(IShape shape)
		{
			Shape = shape;
		}

		public static readonly BindableProperty ShapeProperty =
			BindableProperty.Create(nameof(Shape), typeof(IShape), typeof(ShapeView), null);
		
		public static readonly BindableProperty FillProperty =
			BindableProperty.Create(nameof(Fill), typeof(Paint), typeof(ShapeView), null);

		public static readonly BindableProperty StrokeProperty =
			BindableProperty.Create(nameof(Stroke), typeof(Color), typeof(ShapeView), null);

		public static readonly BindableProperty StrokeThicknessProperty =
			BindableProperty.Create(nameof(StrokeThickness), typeof(double), typeof(ShapeView), 1.0);

		public static readonly BindableProperty StrokeDashPatternProperty =
			BindableProperty.Create(nameof(StrokeDashPattern), typeof(float[]), typeof(ShapeView), null);

		public static readonly BindableProperty StrokeLineCapProperty =
			BindableProperty.Create(nameof(StrokeLineCap), typeof(LineCap), typeof(ShapeView), LineCap.Butt);

		public static readonly BindableProperty StrokeLineJoinProperty =
			BindableProperty.Create(nameof(StrokeLineJoin), typeof(LineJoin), typeof(ShapeView), LineJoin.Miter);

		public static readonly BindableProperty StrokeMiterLimitProperty =
			BindableProperty.Create(nameof(StrokeMiterLimit), typeof(float), typeof(ShapeView), 10.0f);
		
		public static readonly BindableProperty StretchProperty =	
			BindableProperty.Create(nameof(Stretch), typeof(Stretch), typeof(ShapeView), Stretch.None);

		public IShape? Shape
		{
			set { SetValue(ShapeProperty, value); }
			get { return (IShape?)GetValue(ShapeProperty); }
		}

		public Paint? Fill
		{
			set { SetValue(FillProperty, value); }
			get { return (Paint?)GetValue(FillProperty); }
		}

		public Color? Stroke
		{
			set { SetValue(StrokeProperty, value); }
			get { return (Color?)GetValue(StrokeProperty); }
		}

		public double StrokeThickness
		{
			set { SetValue(StrokeThicknessProperty, value); }
			get { return (double)GetValue(StrokeThicknessProperty); }
		}

		public float[] StrokeDashPattern
		{
			set { SetValue(StrokeDashPatternProperty, value); }
			get { return (float[])GetValue(StrokeDashPatternProperty); }
		}

		public LineCap StrokeLineCap
		{
			set { SetValue(StrokeLineCapProperty, value); }
			get { return (LineCap)GetValue(StrokeLineCapProperty); }
		}

		public LineJoin StrokeLineJoin
		{
			set { SetValue(StrokeLineJoinProperty, value); }
			get { return (LineJoin)GetValue(StrokeLineJoinProperty); }
		}

		public float StrokeMiterLimit
		{
			set { SetValue(StrokeMiterLimitProperty, value); }
			get { return (float)GetValue(StrokeMiterLimitProperty); }
		}

		public Stretch Stretch
		{
			set { SetValue(StretchProperty, value); }
			get { return (Stretch)GetValue(StretchProperty); }
		}
	}
}