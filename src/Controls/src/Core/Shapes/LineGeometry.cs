using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineGeometry.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.LineGeometry']/Docs" />
	public class LineGeometry : Geometry
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineGeometry.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public LineGeometry()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineGeometry.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public LineGeometry(Point startPoint, Point endPoint)
		{
			StartPoint = startPoint;
			EndPoint = endPoint;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineGeometry.xml" path="//Member[@MemberName='StartPointProperty']/Docs" />
		public static readonly BindableProperty StartPointProperty =
			BindableProperty.Create(nameof(StartPoint), typeof(Point), typeof(LineGeometry), new Point());

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineGeometry.xml" path="//Member[@MemberName='EndPointProperty']/Docs" />
		public static readonly BindableProperty EndPointProperty =
			BindableProperty.Create(nameof(EndPoint), typeof(Point), typeof(LineGeometry), new Point());

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineGeometry.xml" path="//Member[@MemberName='StartPoint']/Docs" />
		public Point StartPoint
		{
			set { SetValue(StartPointProperty, value); }
			get { return (Point)GetValue(StartPointProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineGeometry.xml" path="//Member[@MemberName='EndPoint']/Docs" />
		public Point EndPoint
		{
			set { SetValue(EndPointProperty, value); }
			get { return (Point)GetValue(EndPointProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/LineGeometry.xml" path="//Member[@MemberName='AppendPath']/Docs" />
		public override void AppendPath(PathF path)
		{
			path.Move((float)StartPoint.X, (float)StartPoint.Y);
			path.LineTo((float)EndPoint.X, (float)EndPoint.Y);
		}
	}
}