#nullable disable
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Ellipse.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Ellipse']/Docs/*" />
	[ElementHandler(typeof(ShapeViewHandler))]
	public sealed partial class Ellipse : Shape, IShape
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Ellipse.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public Ellipse() : base()
		{
			Aspect = Stretch.Fill;
		}

		// TODO this should move to a remapped mapper
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
			var width = WidthForPathComputation;
			var height = HeightForPathComputation;

			var path = new PathF();

			float x = (float)StrokeThickness / 2;
			float y = (float)StrokeThickness / 2;
			float w = (float)(width - StrokeThickness);
			float h = (float)(height - StrokeThickness);

			path.AppendEllipse(x, y, w, h);

			return path;
		}
	}
}