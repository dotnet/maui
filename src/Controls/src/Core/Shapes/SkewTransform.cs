#nullable disable
using System;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/SkewTransform.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.SkewTransform']/Docs/*" />
	public class SkewTransform : Transform
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/SkewTransform.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public SkewTransform()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/SkewTransform.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public SkewTransform(double angleX, double angleY)
		{
			AngleX = angleX;
			AngleY = angleY;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/SkewTransform.xml" path="//Member[@MemberName='.ctor'][3]/Docs/*" />
		public SkewTransform(double angleX, double angleY, double centerX, double centerY)
		{
			AngleX = angleX;
			AngleY = angleY;
			CenterX = centerX;
			CenterY = centerY;
		}

		/// <summary>Bindable property for <see cref="AngleX"/>.</summary>
		public static readonly BindableProperty AngleXProperty =
			BindableProperty.Create(nameof(AngleX), typeof(double), typeof(SkewTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="AngleY"/>.</summary>
		public static readonly BindableProperty AngleYProperty =
			BindableProperty.Create(nameof(AngleY), typeof(double), typeof(SkewTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="CenterX"/>.</summary>
		public static readonly BindableProperty CenterXProperty =
			BindableProperty.Create(nameof(CenterX), typeof(double), typeof(SkewTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="CenterY"/>.</summary>
		public static readonly BindableProperty CenterYProperty =
			BindableProperty.Create(nameof(CenterY), typeof(double), typeof(SkewTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/SkewTransform.xml" path="//Member[@MemberName='AngleX']/Docs/*" />
		public double AngleX
		{
			set { SetValue(AngleXProperty, value); }
			get { return (double)GetValue(AngleXProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/SkewTransform.xml" path="//Member[@MemberName='AngleY']/Docs/*" />
		public double AngleY
		{
			set { SetValue(AngleYProperty, value); }
			get { return (double)GetValue(AngleYProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/SkewTransform.xml" path="//Member[@MemberName='CenterX']/Docs/*" />
		public double CenterX
		{
			set { SetValue(CenterXProperty, value); }
			get { return (double)GetValue(CenterXProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/SkewTransform.xml" path="//Member[@MemberName='CenterY']/Docs/*" />
		public double CenterY
		{
			set { SetValue(CenterYProperty, value); }
			get { return (double)GetValue(CenterYProperty); }
		}

		static void OnTransformPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			(bindable as SkewTransform).OnTransformPropertyChanged();
		}

		void OnTransformPropertyChanged()
		{
			double radiansX = Math.PI * AngleX / 180;
			double radiansY = Math.PI * AngleY / 180;
			double tanX = Math.Tan(radiansX);
			double tanY = Math.Tan(radiansY);

			Value = new Matrix(1, tanY, tanX, 1, -CenterY * tanX, -CenterX * tanY);
		}
	}
}