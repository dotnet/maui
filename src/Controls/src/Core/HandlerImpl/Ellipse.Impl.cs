using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Ellipse.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Ellipse']/Docs" />
	public partial class Ellipse : IShape
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Ellipse.xml" path="//Member[@MemberName='GetPath']/Docs" />
		public override PathF GetPath()
		{
			var path = new PathF();

			float x = (float)StrokeThickness / 2;
			float y = (float)StrokeThickness / 2;
			float w = (float)(Width - StrokeThickness);
			float h = (float)(Height - StrokeThickness);

			path.AppendEllipse(x, y, w, h);

			return path;
		}
	}
}