#nullable disable
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Converters;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// Represents a path segment that draws an elliptical arc between two points.
	/// </summary>
	public class ArcSegment : PathSegment
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ArcSegment"/> class.
		/// </summary>
		public ArcSegment()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ArcSegment"/> class with the specified arc parameters.
		/// </summary>
		/// <param name="point">The endpoint of the arc.</param>
		/// <param name="size">The x- and y-radius of the arc.</param>
		/// <param name="rotationAngle">The rotation angle of the ellipse in degrees.</param>
		/// <param name="sweepDirection">The direction in which the arc is drawn.</param>
		/// <param name="isLargeArc">Whether the arc should be greater than 180 degrees.</param>
		public ArcSegment(Point point, Size size, double rotationAngle, SweepDirection sweepDirection, bool isLargeArc)
		{
			Point = point;
			Size = size;
			RotationAngle = rotationAngle;
			SweepDirection = sweepDirection;
			IsLargeArc = isLargeArc;
		}

		/// <summary>Bindable property for <see cref="Point"/>.</summary>
		public static readonly BindableProperty PointProperty =
			BindableProperty.Create(nameof(Point), typeof(Point), typeof(ArcSegment), new Point(0, 0));

		/// <summary>Bindable property for <see cref="Size"/>.</summary>
		public static readonly BindableProperty SizeProperty =
			BindableProperty.Create(nameof(Size), typeof(Size), typeof(ArcSegment), new Size(0, 0));

		/// <summary>Bindable property for <see cref="RotationAngle"/>.</summary>
		public static readonly BindableProperty RotationAngleProperty =
			BindableProperty.Create(nameof(RotationAngle), typeof(double), typeof(ArcSegment), 0.0);

		/// <summary>Bindable property for <see cref="SweepDirection"/>.</summary>
		public static readonly BindableProperty SweepDirectionProperty =
			BindableProperty.Create(nameof(SweepDirection), typeof(SweepDirection), typeof(ArcSegment), SweepDirection.CounterClockwise);

		/// <summary>Bindable property for <see cref="IsLargeArc"/>.</summary>
		public static readonly BindableProperty IsLargeArcProperty =
			BindableProperty.Create(nameof(IsLargeArc), typeof(bool), typeof(ArcSegment), false);

		/// <summary>
		/// Gets or sets the endpoint of the arc. This is a bindable property.
		/// </summary>
		public Point Point
		{
			set { SetValue(PointProperty, value); }
			get { return (Point)GetValue(PointProperty); }
		}

		/// <summary>
		/// Gets or sets the x- and y-radius of the arc. This is a bindable property.
		/// </summary>
		[System.ComponentModel.TypeConverter(typeof(SizeTypeConverter))]
		public Size Size
		{
			set { SetValue(SizeProperty, value); }
			get { return (Size)GetValue(SizeProperty); }
		}

		/// <summary>
		/// Gets or sets the rotation angle of the ellipse in degrees. This is a bindable property.
		/// </summary>
		public double RotationAngle
		{
			set { SetValue(RotationAngleProperty, value); }
			get { return (double)GetValue(RotationAngleProperty); }
		}

		/// <summary>
		/// Gets or sets the direction in which the arc is drawn (clockwise or counterclockwise). This is a bindable property.
		/// </summary>
		public SweepDirection SweepDirection
		{
			set { SetValue(SweepDirectionProperty, value); }
			get { return (SweepDirection)GetValue(SweepDirectionProperty); }
		}

		/// <summary>
		/// Gets or sets whether the arc should be greater than 180 degrees. This is a bindable property.
		/// </summary>
		public bool IsLargeArc
		{
			set { SetValue(IsLargeArcProperty, value); }
			get { return (bool)GetValue(IsLargeArcProperty); }
		}
	}
}
