using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Ellipse.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Ellipse']/Docs/*" />
	public partial class Ellipse : IShape
	{
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == XProperty.PropertyName ||
				propertyName == YProperty.PropertyName ||
				propertyName == WidthProperty.PropertyName ||
				propertyName == HeightProperty.PropertyName)
			{
				Handler?.UpdateValue(nameof(IShapeView.Shape));
			}
		}

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
