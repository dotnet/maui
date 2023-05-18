#nullable disable
namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/CompositeTransform.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.CompositeTransform']/Docs/*" />
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

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/CompositeTransform.xml" path="//Member[@MemberName='CenterX']/Docs/*" />
		public double CenterX
		{
			set { SetValue(CenterXProperty, value); }
			get { return (double)GetValue(CenterXProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/CompositeTransform.xml" path="//Member[@MemberName='CenterY']/Docs/*" />
		public double CenterY
		{
			set { SetValue(CenterYProperty, value); }
			get { return (double)GetValue(CenterYProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/CompositeTransform.xml" path="//Member[@MemberName='ScaleX']/Docs/*" />
		public double ScaleX
		{
			set { SetValue(ScaleXProperty, value); }
			get { return (double)GetValue(ScaleXProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/CompositeTransform.xml" path="//Member[@MemberName='ScaleY']/Docs/*" />
		public double ScaleY
		{
			set { SetValue(ScaleYProperty, value); }
			get { return (double)GetValue(ScaleYProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/CompositeTransform.xml" path="//Member[@MemberName='SkewX']/Docs/*" />
		public double SkewX
		{
			set { SetValue(SkewXProperty, value); }
			get { return (double)GetValue(SkewXProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/CompositeTransform.xml" path="//Member[@MemberName='SkewY']/Docs/*" />
		public double SkewY
		{
			set { SetValue(SkewYProperty, value); }
			get { return (double)GetValue(SkewYProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/CompositeTransform.xml" path="//Member[@MemberName='Rotation']/Docs/*" />
		public double Rotation
		{
			set { SetValue(RotationProperty, value); }
			get { return (double)GetValue(RotationProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/CompositeTransform.xml" path="//Member[@MemberName='TranslateX']/Docs/*" />
		public double TranslateX
		{
			set { SetValue(TranslateXProperty, value); }
			get { return (double)GetValue(TranslateXProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/CompositeTransform.xml" path="//Member[@MemberName='TranslateY']/Docs/*" />
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