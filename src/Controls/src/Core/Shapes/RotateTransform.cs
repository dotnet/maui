using System;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RotateTransform.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.RotateTransform']/Docs" />
	public class RotateTransform : Transform
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RotateTransform.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public RotateTransform()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RotateTransform.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public RotateTransform(double angle)
		{
			Angle = angle;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RotateTransform.xml" path="//Member[@MemberName='.ctor'][3]/Docs" />
		public RotateTransform(double angle, double centerX, double centerY)
		{
			Angle = angle;
			CenterX = centerX;
			CenterY = centerY;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RotateTransform.xml" path="//Member[@MemberName='AngleProperty']/Docs" />
		public static readonly BindableProperty AngleProperty =
			BindableProperty.Create(nameof(Angle), typeof(double), typeof(RotateTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RotateTransform.xml" path="//Member[@MemberName='CenterXProperty']/Docs" />
		public static readonly BindableProperty CenterXProperty =
			BindableProperty.Create(nameof(CenterX), typeof(double), typeof(RotateTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RotateTransform.xml" path="//Member[@MemberName='CenterYProperty']/Docs" />
		public static readonly BindableProperty CenterYProperty =
			BindableProperty.Create(nameof(CenterY), typeof(double), typeof(RotateTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RotateTransform.xml" path="//Member[@MemberName='Angle']/Docs" />
		public double Angle
		{
			set { SetValue(AngleProperty, value); }
			get { return (double)GetValue(AngleProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RotateTransform.xml" path="//Member[@MemberName='CenterX']/Docs" />
		public double CenterX
		{
			set { SetValue(CenterXProperty, value); }
			get { return (double)GetValue(CenterXProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RotateTransform.xml" path="//Member[@MemberName='CenterY']/Docs" />
		public double CenterY
		{
			set { SetValue(CenterYProperty, value); }
			get { return (double)GetValue(CenterYProperty); }
		}

		static void OnTransformPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as RotateTransform).OnTransformPropertyChanged();
		}

		void OnTransformPropertyChanged()
		{
			double radians = Math.PI * Angle / 180;
			double sin = Math.Sin(radians);
			double cos = Math.Cos(radians);

			Value = new Matrix(cos, sin, -sin, cos, CenterX * (1 - cos) + CenterY * sin, CenterY * (1 - cos) - CenterX * sin);
		}
	}
}