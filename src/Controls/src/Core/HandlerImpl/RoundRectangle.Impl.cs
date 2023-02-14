#nullable disable
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

		PathF IRoundRectangle.InnerPathForBounds(Rect viewBounds, float strokeWidth)
		{
			var path = GetInnerPath(strokeWidth);

			TransformPathForBounds(path, viewBounds);

			return path;
		}

		internal PathF GetInnerPath(float strokeWidth)
		{
			var path = new PathF();

			float x = (float)strokeWidth / 2;
			float y = (float)strokeWidth / 2;

			float w = (float)(Width - strokeWidth);
			float h = (float)(Height - strokeWidth);

			float topLeftCornerRadius = (float)Math.Max(0, CornerRadius.TopLeft - strokeWidth);
			float topRightCornerRadius = (float)Math.Max(0, CornerRadius.TopRight - strokeWidth);
			float bottomLeftCornerRadius = (float)Math.Max(0, CornerRadius.BottomLeft - strokeWidth);
			float bottomRightCornerRadius = (float)Math.Max(0, CornerRadius.BottomRight - strokeWidth);

			path.AppendRoundedRectangle(x, y, w, h, topLeftCornerRadius, topRightCornerRadius, bottomLeftCornerRadius, bottomRightCornerRadius);

			return path;
		}
	}
}