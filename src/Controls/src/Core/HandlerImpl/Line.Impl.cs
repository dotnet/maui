using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Line.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Line']/Docs/*" />
	public partial class Line : IShape
	{
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == X1Property.PropertyName ||
				propertyName == Y1Property.PropertyName ||
				propertyName == X2Property.PropertyName ||
				propertyName == Y2Property.PropertyName)
				Handler?.UpdateValue(nameof(IShapeView.Shape));
		}

		public override PathF GetPath()
		{
			var path = new PathF();

			path.MoveTo((float)X1, (float)Y1);
			path.LineTo((float)X2, (float)Y2);

			return path;
		}
	}
}
