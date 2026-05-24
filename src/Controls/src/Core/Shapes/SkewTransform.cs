#nullable disable
using System;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// A transform that skews (shears) an element by the specified angles.
	/// </summary>
	public class SkewTransform : Transform
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SkewTransform"/> class.
		/// </summary>
		public SkewTransform()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SkewTransform"/> class with the specified skew angles.
		/// </summary>
		/// <param name="angleX">The x-axis skew angle in degrees.</param>
		/// <param name="angleY">The y-axis skew angle in degrees.</param>
		public SkewTransform(double angleX, double angleY)
		{
			AngleX = angleX;
			AngleY = angleY;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SkewTransform"/> class with the specified skew angles and center point.
		/// </summary>
		/// <param name="angleX">The x-axis skew angle in degrees.</param>
		/// <param name="angleY">The y-axis skew angle in degrees.</param>
		/// <param name="centerX">The x-coordinate of the skew center point.</param>
		/// <param name="centerY">The y-coordinate of the skew center point.</param>
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

		/// <summary>
		/// Gets or sets the x-axis skew angle in degrees. This is a bindable property.
		/// </summary>
		public double AngleX
		{
			set { SetValue(AngleXProperty, value); }
			get { return (double)GetValue(AngleXProperty); }
		}

		/// <summary>
		/// Gets or sets the y-axis skew angle in degrees. This is a bindable property.
		/// </summary>
		public double AngleY
		{
			set { SetValue(AngleYProperty, value); }
			get { return (double)GetValue(AngleYProperty); }
		}

		/// <summary>
		/// Gets or sets the x-coordinate of the skew origin. This is a bindable property.
		/// </summary>
		public double CenterX
		{
			set { SetValue(CenterXProperty, value); }
			get { return (double)GetValue(CenterXProperty); }
		}

		/// <summary>
		/// Gets or sets the y-coordinate of the skew origin. This is a bindable property.
		/// </summary>
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