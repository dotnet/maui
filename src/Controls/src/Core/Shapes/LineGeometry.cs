using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineGeometry.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.LineGeometry']/Docs/*" />
	public class LineGeometry : Geometry
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineGeometry.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public LineGeometry()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineGeometry.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public LineGeometry(Point startPoint, Point endPoint)
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineGeometry.xml" path="//Member[@MemberName='StartPointProperty']/Docs/*" />
		public static readonly BindableProperty StartPointProperty =
			BindableProperty.Create(nameof(StartPoint), typeof(Point), typeof(LineGeometry), new Point());

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineGeometry.xml" path="//Member[@MemberName='EndPointProperty']/Docs/*" />
		public static readonly BindableProperty EndPointProperty =
			BindableProperty.Create(nameof(EndPoint), typeof(Point), typeof(LineGeometry), new Point());

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineGeometry.xml" path="//Member[@MemberName='StartPoint']/Docs/*" />
		public Point StartPoint
		{
			set { SetValue(StartPointProperty, value); }
			get { return (Point)GetValue(StartPointProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineGeometry.xml" path="//Member[@MemberName='EndPoint']/Docs/*" />
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
