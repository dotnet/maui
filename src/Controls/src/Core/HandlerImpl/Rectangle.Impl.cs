using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/Rectangle.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.Rectangle']/Docs/*" />
	public partial class Rectangle : IShape
	{
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == RadiusXProperty.PropertyName ||
				propertyName == RadiusYProperty.PropertyName)
				Handler?.UpdateValue(nameof(IShapeView.Shape));
		}

		public override PathF GetPath()
		{
			var path = new PathF();

			float x = (float)StrokeThickness / 2;
			float y = (float)StrokeThickness / 2;
			float w = (float)(Width - StrokeThickness);
			float h = (float)(Height - StrokeThickness);
			float cornerRadius = (float)Math.Max(RadiusX, RadiusY);

			// TODO: Create specific Path taking into account RadiusX and RadiusY
			path.AppendRoundedRectangle(x, y, w, h, cornerRadius);

			return path;
		}
	}
}
