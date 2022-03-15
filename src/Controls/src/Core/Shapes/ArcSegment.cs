using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Converters;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ArcSegment.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.ArcSegment']/Docs" />
	public class ArcSegment : PathSegment
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ArcSegment.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public ArcSegment()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ArcSegment.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public ArcSegment(Point point, Size size, double rotationAngle, SweepDirection sweepDirection, bool isLargeArc)
		{
			Point = point;
			Size = size;
			RotationAngle = rotationAngle;
			SweepDirection = sweepDirection;
			IsLargeArc = isLargeArc;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ArcSegment.xml" path="//Member[@MemberName='PointProperty']/Docs" />
		public static readonly BindableProperty PointProperty =
			BindableProperty.Create(nameof(Point), typeof(Point), typeof(ArcSegment), new Point(0, 0));

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ArcSegment.xml" path="//Member[@MemberName='SizeProperty']/Docs" />
		public static readonly BindableProperty SizeProperty =
			BindableProperty.Create(nameof(Size), typeof(Size), typeof(ArcSegment), new Size(0, 0));

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ArcSegment.xml" path="//Member[@MemberName='RotationAngleProperty']/Docs" />
		public static readonly BindableProperty RotationAngleProperty =
			BindableProperty.Create(nameof(RotationAngle), typeof(double), typeof(ArcSegment), 0.0);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ArcSegment.xml" path="//Member[@MemberName='SweepDirectionProperty']/Docs" />
		public static readonly BindableProperty SweepDirectionProperty =
			BindableProperty.Create(nameof(SweepDirection), typeof(SweepDirection), typeof(ArcSegment), SweepDirection.CounterClockwise);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ArcSegment.xml" path="//Member[@MemberName='IsLargeArcProperty']/Docs" />
		public static readonly BindableProperty IsLargeArcProperty =
			BindableProperty.Create(nameof(IsLargeArc), typeof(bool), typeof(ArcSegment), false);

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ArcSegment.xml" path="//Member[@MemberName='Point']/Docs" />
		public Point Point
		{
			set { SetValue(PointProperty, value); }
			get { return (Point)GetValue(PointProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ArcSegment.xml" path="//Member[@MemberName='Size']/Docs" />
		[System.ComponentModel.TypeConverter(typeof(SizeTypeConverter))]
		public Size Size
		{
			set { SetValue(SizeProperty, value); }
			get { return (Size)GetValue(SizeProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ArcSegment.xml" path="//Member[@MemberName='RotationAngle']/Docs" />
		public double RotationAngle
		{
			set { SetValue(RotationAngleProperty, value); }
			get { return (double)GetValue(RotationAngleProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ArcSegment.xml" path="//Member[@MemberName='SweepDirection']/Docs" />
		public SweepDirection SweepDirection
		{
			set { SetValue(SweepDirectionProperty, value); }
			get { return (SweepDirection)GetValue(SweepDirectionProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/ArcSegment.xml" path="//Member[@MemberName='IsLargeArc']/Docs" />
		public bool IsLargeArc
		{
			set { SetValue(IsLargeArcProperty, value); }
			get { return (bool)GetValue(IsLargeArcProperty); }
		}
	}
}
