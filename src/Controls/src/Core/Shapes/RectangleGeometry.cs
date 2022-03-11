using Rect = Microsoft.Maui.Graphics.Rect;
namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RectangleGeometry.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.RectangleGeometry']/Docs" />
	public class RectangleGeometry : Geometry
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RectangleGeometry.xml" path="//Member[@MemberName='.ctor'][0]/Docs" />
		public RectangleGeometry()
		{

		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RectangleGeometry.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public RectangleGeometry(Rect rect)
		{
			Rect = rect;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RectangleGeometry.xml" path="//Member[@MemberName='RectProperty']/Docs" />
		public static readonly BindableProperty RectProperty =
			BindableProperty.Create(nameof(Rect), typeof(Rect), typeof(RectangleGeometry), new Rect());

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RectangleGeometry.xml" path="//Member[@MemberName='Rect']/Docs" />
		public Rect Rect
		{
			set { SetValue(RectProperty, value); }
			get { return (Rect)GetValue(RectProperty); }
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/RectangleGeometry.xml" path="//Member[@MemberName='AppendPath']/Docs" />
		public override void AppendPath(Graphics.PathF path)
		{
			path.AppendRectangle(Rect);
		}
	}
}