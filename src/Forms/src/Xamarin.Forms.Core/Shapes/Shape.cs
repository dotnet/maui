namespace Xamarin.Forms.Shapes
{
	public abstract class Shape : View
	{
		public Shape()
		{
		}

		public static readonly BindableProperty FillProperty =
			BindableProperty.Create(nameof(Fill), typeof(Brush), typeof(Shape), null,
				propertyChanged: OnBrushChanged);

		public static readonly BindableProperty StrokeProperty =
			BindableProperty.Create(nameof(Stroke), typeof(Brush), typeof(Shape), null,
				propertyChanged: OnBrushChanged);

		public static readonly BindableProperty StrokeThicknessProperty =
			BindableProperty.Create(nameof(StrokeThickness), typeof(double), typeof(Shape), 1.0);

		public static readonly BindableProperty StrokeDashArrayProperty =
			BindableProperty.Create(nameof(StrokeDashArray), typeof(DoubleCollection), typeof(Shape), null,
				defaultValueCreator: bindable => new DoubleCollection());

		public static readonly BindableProperty StrokeDashOffsetProperty =
			BindableProperty.Create(nameof(StrokeDashOffset), typeof(double), typeof(Shape), 0.0);

		public static readonly BindableProperty StrokeLineCapProperty =
			BindableProperty.Create(nameof(StrokeLineCap), typeof(PenLineCap), typeof(Shape), PenLineCap.Flat);

		public static readonly BindableProperty StrokeLineJoinProperty =
			BindableProperty.Create(nameof(StrokeLineJoin), typeof(PenLineJoin), typeof(Shape), PenLineJoin.Miter);

		public static readonly BindableProperty StrokeMiterLimitProperty =
			BindableProperty.Create(nameof(StrokeMiterLimit), typeof(double), typeof(Shape), 10.0);

		public static readonly BindableProperty AspectProperty =
			BindableProperty.Create(nameof(Aspect), typeof(Stretch), typeof(Shape), Stretch.None);

		public Brush Fill
		{
			set { SetValue(FillProperty, value); }
			get { return (Brush)GetValue(FillProperty); }
		}

		public Brush Stroke
		{
			set { SetValue(StrokeProperty, value); }
			get { return (Brush)GetValue(StrokeProperty); }
		}

		public double StrokeThickness
		{
			set { SetValue(StrokeThicknessProperty, value); }
			get { return (double)GetValue(StrokeThicknessProperty); }
		}

		public DoubleCollection StrokeDashArray
		{
			set { SetValue(StrokeDashArrayProperty, value); }
			get { return (DoubleCollection)GetValue(StrokeDashArrayProperty); }
		}

		public double StrokeDashOffset
		{
			set { SetValue(StrokeDashOffsetProperty, value); }
			get { return (double)GetValue(StrokeDashOffsetProperty); }
		}

		public PenLineCap StrokeLineCap
		{
			set { SetValue(StrokeLineCapProperty, value); }
			get { return (PenLineCap)GetValue(StrokeLineCapProperty); }
		}

		public PenLineJoin StrokeLineJoin
		{
			set { SetValue(StrokeLineJoinProperty, value); }
			get { return (PenLineJoin)GetValue(StrokeLineJoinProperty); }
		}

		public double StrokeMiterLimit
		{
			set { SetValue(StrokeMiterLimitProperty, value); }
			get { return (double)GetValue(StrokeMiterLimitProperty); }
		}

		public Stretch Aspect
		{
			set { SetValue(AspectProperty, value); }
			get { return (Stretch)GetValue(AspectProperty); }
		}

		static void OnBrushChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((Shape)bindable).UpdateBrushParent((Brush)newValue);
		}

		void UpdateBrushParent(Brush brush)
		{
			if (brush != null)
				brush.Parent = this;
		}
	}
}