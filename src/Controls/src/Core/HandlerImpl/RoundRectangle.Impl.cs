using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class RoundRectangle : IShape
	{
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == CornerRadiusProperty.PropertyName)
				Handler?.UpdateValue(nameof(IShapeView.Shape));
		}

		public override PathF GetPath()
		{
			var path = new PathF();

			float x = (float)StrokeThickness / 2;
			float y = (float)StrokeThickness / 2;

			float w = (float)(Width - StrokeThickness);
			float h = (float)(Height - StrokeThickness);

			float topLeftCornerRadius = (float)CornerRadius.TopLeft;
			float topRightCornerRadius = (float)CornerRadius.TopRight;
			float bottomLeftCornerRadius = (float)CornerRadius.BottomLeft;
			float bottomRightCornerRadius = (float)CornerRadius.BottomRight;

			path.AppendRoundedRectangle(x, y, w, h, topLeftCornerRadius, topRightCornerRadius, bottomLeftCornerRadius, bottomRightCornerRadius);

			return path;
		}
	}
}