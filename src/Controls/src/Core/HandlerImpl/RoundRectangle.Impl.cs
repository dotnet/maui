using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class RoundRectangle : IRoundRectangle
	{
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == CornerRadiusProperty.PropertyName)
				Handler?.UpdateValue(nameof(IShapeView.Shape));
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

			float topLeftCornerRadius = (float)CornerRadius.TopLeft;
			float topRightCornerRadius = (float)CornerRadius.TopRight;
			float bottomLeftCornerRadius = (float)CornerRadius.BottomLeft;
			float bottomRightCornerRadius = (float)CornerRadius.BottomRight;

			path.AppendRoundedRectangle(x, y, w, h, topLeftCornerRadius, topRightCornerRadius, bottomLeftCornerRadius, bottomRightCornerRadius);

			return path;
		}

		internal PathF GetClipPath(float strokeThickness)
		{
			var width = WidthForPathComputation;
			var height = HeightForPathComputation;

			var path = new PathF();

			float x = (float)strokeThickness / 2;
			float y = (float)strokeThickness / 2;

			float w = (float)(width - strokeThickness);
			float h = (float)(height - strokeThickness);

			float topLeftCornerRadius = (float)CornerRadius.TopLeft;
			float topRightCornerRadius = (float)CornerRadius.TopRight;
			float bottomLeftCornerRadius = (float)CornerRadius.BottomLeft;
			float bottomRightCornerRadius = (float)CornerRadius.BottomRight;

			// The equation to determine the inner corner radius:
			// Inner Corner Radius = Corner Radius - StrokeTickness

			float innerTopLeftCornerRadius = Math.Max(topLeftCornerRadius - strokeThickness, 0);
			float innerTopRightCornerRadius = Math.Max(topRightCornerRadius - strokeThickness, 0);
			float innerBottomLeftCornerRadius = Math.Max(bottomLeftCornerRadius - strokeThickness, 0);
			float innerBottomRightCornerRadius = Math.Max(bottomRightCornerRadius - strokeThickness, 0);

			path.AppendRoundedRectangle(x, y, w, h, innerTopLeftCornerRadius, innerTopRightCornerRadius, innerBottomLeftCornerRadius, innerBottomRightCornerRadius);

			return path;
		}

		PathF IRoundRectangle.ClipPathForBounds(Graphics.Rect viewBounds, double strokeThickness)
		{
			var path = GetClipPath((float)strokeThickness);

			base.UpdateAspect(path, viewBounds);

			return path;
		}
	}
}