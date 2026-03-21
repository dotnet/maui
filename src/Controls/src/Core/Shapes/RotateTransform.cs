#nullable disable
using System;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// A transform that rotates an element around a specified center point.
	/// </summary>
	public class RotateTransform : Transform
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RotateTransform"/> class.
		/// </summary>
		public RotateTransform()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RotateTransform"/> class with the specified angle.
		/// </summary>
		/// <param name="angle">The rotation angle in degrees.</param>
		public RotateTransform(double angle)
		{
			Angle = angle;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RotateTransform"/> class with the specified angle and center point.
		/// </summary>
		/// <param name="angle">The rotation angle in degrees.</param>
		/// <param name="centerX">The x-coordinate of the center of rotation.</param>
		/// <param name="centerY">The y-coordinate of the center of rotation.</param>
		public RotateTransform(double angle, double centerX, double centerY)
		{
			Angle = angle;
			CenterX = centerX;
			CenterY = centerY;
		}

		/// <summary>Bindable property for <see cref="Angle"/>.</summary>
		public static readonly BindableProperty AngleProperty =
			BindableProperty.Create(nameof(Angle), typeof(double), typeof(RotateTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="CenterX"/>.</summary>
		public static readonly BindableProperty CenterXProperty =
			BindableProperty.Create(nameof(CenterX), typeof(double), typeof(RotateTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>Bindable property for <see cref="CenterY"/>.</summary>
		public static readonly BindableProperty CenterYProperty =
			BindableProperty.Create(nameof(CenterY), typeof(double), typeof(RotateTransform), 0.0,
				propertyChanged: OnTransformPropertyChanged);

		/// <summary>
		/// Gets or sets the rotation angle in degrees. This is a bindable property.
		/// </summary>
		public double Angle
		{
			set { SetValue(AngleProperty, value); }
			get { return (double)GetValue(AngleProperty); }
		}

		/// <summary>
		/// Gets or sets the x-coordinate of the center of rotation. This is a bindable property.
		/// </summary>
		public double CenterX
		{
			set { SetValue(CenterXProperty, value); }
			get { return (double)GetValue(CenterXProperty); }
		}

		/// <summary>
		/// Gets or sets the y-coordinate of the center of rotation. This is a bindable property.
		/// </summary>
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