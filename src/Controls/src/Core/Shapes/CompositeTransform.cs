#nullable disable
namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// A transform that combines multiple transform operations (scale, skew, rotate, translate) into a single transform.
	/// </summary>
	public sealed class CompositeTransform : Transform
	{
		/// <summary>Bindable property for <see cref="CenterX"/>.</summary>
		public static readonly BindableProperty CenterXProperty =
			BindableProperty.Create(nameof(CenterX), typeof(double), typeof(CompositeTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="CenterY"/>.</summary>
		public static readonly BindableProperty CenterYProperty =
			BindableProperty.Create(nameof(CenterY), typeof(double), typeof(CompositeTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="ScaleX"/>.</summary>
		public static readonly BindableProperty ScaleXProperty =
			BindableProperty.Create(nameof(ScaleX), typeof(double), typeof(CompositeTransform), 1.0, propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="ScaleY"/>.</summary>
		public static readonly BindableProperty ScaleYProperty =
			BindableProperty.Create(nameof(ScaleY), typeof(double), typeof(CompositeTransform), 1.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="SkewX"/>.</summary>
		public static readonly BindableProperty SkewXProperty =
			BindableProperty.Create(nameof(SkewX), typeof(double), typeof(CompositeTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="SkewY"/>.</summary>
		public static readonly BindableProperty SkewYProperty =
			BindableProperty.Create(nameof(SkewY), typeof(double), typeof(CompositeTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="Rotation"/>.</summary>
		public static readonly BindableProperty RotationProperty =
			BindableProperty.Create(nameof(Rotation), typeof(double), typeof(CompositeTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="TranslateX"/>.</summary>
		public static readonly BindableProperty TranslateXProperty =
			BindableProperty.Create(nameof(TranslateX), typeof(double), typeof(CompositeTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="TranslateY"/>.</summary>
		public static readonly BindableProperty TranslateYProperty =
			BindableProperty.Create(nameof(TranslateY), typeof(double), typeof(CompositeTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>
		/// Gets or sets the x-coordinate of the center point for all transforms. This is a bindable property.
		/// </summary>
		public double CenterX
		{
			set { SetValue(CenterXProperty, value); }
			get { return (double)GetValue(CenterXProperty); }
		}

		/// <summary>
		/// Gets or sets the y-coordinate of the center point for all transforms. This is a bindable property.
		/// </summary>
		public double CenterY
		{
			set { SetValue(CenterYProperty, value); }
			get { return (double)GetValue(CenterYProperty); }
		}

		/// <summary>
		/// Gets or sets the x-axis scale factor. This is a bindable property.
		/// </summary>
		public double ScaleX
		{
			set { SetValue(ScaleXProperty, value); }
			get { return (double)GetValue(ScaleXProperty); }
		}

		/// <summary>
		/// Gets or sets the y-axis scale factor. This is a bindable property.
		/// </summary>
		public double ScaleY
		{
			set { SetValue(ScaleYProperty, value); }
			get { return (double)GetValue(ScaleYProperty); }
		}

		/// <summary>
		/// Gets or sets the x-axis skew angle, in degrees. This is a bindable property.
		/// </summary>
		public double SkewX
		{
			set { SetValue(SkewXProperty, value); }
			get { return (double)GetValue(SkewXProperty); }
		}

		/// <summary>
		/// Gets or sets the y-axis skew angle, in degrees. This is a bindable property.
		/// </summary>
		public double SkewY
		{
			set { SetValue(SkewYProperty, value); }
			get { return (double)GetValue(SkewYProperty); }
		}

		/// <summary>
		/// Gets or sets the rotation angle, in degrees. This is a bindable property.
		/// </summary>
		public double Rotation
		{
			set { SetValue(RotationProperty, value); }
			get { return (double)GetValue(RotationProperty); }
		}

		/// <summary>
		/// Gets or sets the x-axis translation offset. This is a bindable property.
		/// </summary>
		public double TranslateX
		{
			set { SetValue(TranslateXProperty, value); }
			get { return (double)GetValue(TranslateXProperty); }
		}

		/// <summary>
		/// Gets or sets the y-axis translation offset. This is a bindable property.
		/// </summary>
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