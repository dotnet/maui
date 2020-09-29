namespace Xamarin.Forms.Shapes
{
	public sealed class CompositeTransform : Transform
	{
		public static readonly BindableProperty CenterXProperty =
			BindableProperty.Create(nameof(CenterX), typeof(double), typeof(CompositeTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		public static readonly BindableProperty CenterYProperty =
			BindableProperty.Create(nameof(CenterY), typeof(double), typeof(CompositeTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		public static readonly BindableProperty ScaleXProperty =
			BindableProperty.Create(nameof(ScaleX), typeof(double), typeof(CompositeTransform), 1.0, propertyChanged: OnTransformPropertyChanged);

		public static readonly BindableProperty ScaleYProperty =
			BindableProperty.Create(nameof(ScaleY), typeof(double), typeof(CompositeTransform), 1.0,
				propertyChanged: OnTransformPropertyChanged);

		public static readonly BindableProperty SkewXProperty =
			BindableProperty.Create(nameof(SkewX), typeof(double), typeof(CompositeTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		public static readonly BindableProperty SkewYProperty =
			BindableProperty.Create(nameof(SkewY), typeof(double), typeof(CompositeTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		public static readonly BindableProperty RotationProperty =
			BindableProperty.Create(nameof(Rotation), typeof(double), typeof(CompositeTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		public static readonly BindableProperty TranslateXProperty =
			BindableProperty.Create(nameof(TranslateX), typeof(double), typeof(CompositeTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		public static readonly BindableProperty TranslateYProperty =
			BindableProperty.Create(nameof(TranslateY), typeof(double), typeof(CompositeTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		public double CenterX
		{
			set { SetValue(CenterXProperty, value); }
			get { return (double)GetValue(CenterXProperty); }
		}

		public double CenterY
		{
			set { SetValue(CenterYProperty, value); }
			get { return (double)GetValue(CenterYProperty); }
		}

		public double ScaleX
		{
			set { SetValue(ScaleXProperty, value); }
			get { return (double)GetValue(ScaleXProperty); }
		}

		public double ScaleY
		{
			set { SetValue(ScaleYProperty, value); }
			get { return (double)GetValue(ScaleYProperty); }
		}

		public double SkewX
		{
			set { SetValue(SkewXProperty, value); }
			get { return (double)GetValue(SkewXProperty); }
		}

		public double SkewY
		{
			set { SetValue(SkewYProperty, value); }
			get { return (double)GetValue(SkewYProperty); }
		}

		public double Rotation
		{
			set { SetValue(RotationProperty, value); }
			get { return (double)GetValue(RotationProperty); }
		}

		public double TranslateX
		{
			set { SetValue(TranslateXProperty, value); }
			get { return (double)GetValue(TranslateXProperty); }
		}

		public double TranslateY
		{
			set { SetValue(TranslateYProperty, value); }
			get { return (double)GetValue(TranslateYProperty); }
		}

		static void OnTransformPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as CompositeTransform).OnTransformPropertyChanged();
		}

		void OnTransformPropertyChanged()
		{
			TransformGroup xformGroup = new TransformGroup
			{
				Children =
				{
					new TranslateTransform
					{
						X = -CenterX,
						Y = -CenterY
					},
					new ScaleTransform
					{
						ScaleX = ScaleX,
						ScaleY = ScaleY
					},
					new SkewTransform
					{
						AngleX = SkewX,
						AngleY = SkewY
					},
					new RotateTransform
					{
						Angle = Rotation
					},
					new TranslateTransform
					{
						X = CenterX,
						Y = CenterY
					},
					new TranslateTransform
					{
						X = TranslateX,
						Y = TranslateY
					}
				}
			};

			Value = xformGroup.Value;
		}
	}
}