namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ScaleTransform.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.ScaleTransform']/Docs" />
	public class ScaleTransform : Transform
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ScaleTransform.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public ScaleTransform()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ScaleTransform.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public ScaleTransform(double scaleX, double scaleY)
		{
			ScaleX = scaleX;
			ScaleY = scaleY;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ScaleTransform.xml" path="//Member[@MemberName='.ctor'][3]/Docs" />
		public ScaleTransform(double scaleX, double scaleY, double centerX, double centerY)
		{
			ScaleX = scaleX;
			ScaleY = scaleY;
			CenterX = centerX;
			CenterY = centerY;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ScaleTransform.xml" path="//Member[@MemberName='ScaleXProperty']/Docs" />
		public static readonly BindableProperty ScaleXProperty =
			BindableProperty.Create(nameof(ScaleX), typeof(double), typeof(ScaleTransform), 1.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ScaleTransform.xml" path="//Member[@MemberName='ScaleYProperty']/Docs" />
		public static readonly BindableProperty ScaleYProperty =
			BindableProperty.Create(nameof(ScaleY), typeof(double), typeof(ScaleTransform), 1.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ScaleTransform.xml" path="//Member[@MemberName='CenterXProperty']/Docs" />
		public static readonly BindableProperty CenterXProperty =
			BindableProperty.Create(nameof(CenterX), typeof(double), typeof(ScaleTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ScaleTransform.xml" path="//Member[@MemberName='CenterYProperty']/Docs" />
		public static readonly BindableProperty CenterYProperty =
			BindableProperty.Create(nameof(CenterY), typeof(double), typeof(ScaleTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ScaleTransform.xml" path="//Member[@MemberName='ScaleX']/Docs" />
		public double ScaleX
		{
			set { SetValue(ScaleXProperty, value); }
			get { return (double)GetValue(ScaleXProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ScaleTransform.xml" path="//Member[@MemberName='ScaleY']/Docs" />
		public double ScaleY
		{
			set { SetValue(ScaleYProperty, value); }
			get { return (double)GetValue(ScaleYProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ScaleTransform.xml" path="//Member[@MemberName='CenterX']/Docs" />
		public double CenterX
		{
			set { SetValue(CenterXProperty, value); }
			get { return (double)GetValue(CenterXProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ScaleTransform.xml" path="//Member[@MemberName='CenterY']/Docs" />
		public double CenterY
		{
			set { SetValue(CenterYProperty, value); }
			get { return (double)GetValue(CenterYProperty); }
		}

		static void OnTransformPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as ScaleTransform).OnTransformPropertyChanged();
		}

		void OnTransformPropertyChanged()
		{
			Value = new Matrix(ScaleX, 0, 0, ScaleY, CenterX * (1 - ScaleX), CenterY * (1 - ScaleY));
		}
	}
}