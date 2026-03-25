#nullable disable
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <summary>
	/// Represents the geometry of a line.
	/// </summary>
	public class LineGeometry : Geometry
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LineGeometry"/> class.
		/// </summary>
		public LineGeometry()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LineGeometry"/> class with the specified start and end points.
		/// </summary>
		/// <param name="startPoint">The starting point of the line.</param>
		/// <param name="endPoint">The ending point of the line.</param>
		public LineGeometry(Point startPoint, Point endPoint)
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		/// <summary>Bindable property for <see cref="StartPoint"/>.</summary>
		public static readonly BindableProperty StartPointProperty =
			BindableProperty.Create(nameof(StartPoint), typeof(Point), typeof(LineGeometry), new Point());

		/// <summary>Bindable property for <see cref="EndPoint"/>.</summary>
		public static readonly BindableProperty EndPointProperty =
			BindableProperty.Create(nameof(EndPoint), typeof(Point), typeof(LineGeometry), new Point());

		/// <summary>
		/// Gets or sets the starting point of the line. This is a bindable property.
		/// </summary>
		public Point StartPoint
		{
			set { SetValue(StartPointProperty, value); }
			get { return (Point)GetValue(StartPointProperty); }
		}

		/// <summary>
		/// Gets or sets the ending point of the line. This is a bindable property.
		/// </summary>
		public Point EndPoint
		{
			set { SetValue(EndPointProperty, value); }
			get { return (Point)GetValue(EndPointProperty); }
		}

		public override void AppendPath(PathF path)
		{
			float startPointX = (float)StartPoint.X;
			float startPointY = (float)StartPoint.Y;

			float endPointX = (float)EndPoint.X;
			float endPointY = (float)EndPoint.Y;

			path.Move(startPointX, startPointY);
			path.LineTo(endPointX, endPointY);
		}
	}
}
