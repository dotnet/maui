#nullable disable
using System;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	public partial class RoundRectangle : IRoundRectangle
	{
		double _fallbackWidth;
		double _fallbackHeight;

		internal override double WidthForPathComputation
		{
			get
			{
				var width = Width;

				return width == -1 ? _fallbackWidth : width;
			}
		}

		internal override double HeightForPathComputation
		{
			get
			{
				var height = Height;

				return height == -1 ? _fallbackHeight : height;
			}
		}

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

			float density = 1f;
#if ANDROID
			density = (float)DeviceDisplay.MainDisplayInfo.Density;
#endif
			float topLeftCornerRadius = (float)CornerRadius.TopLeft * density;
			float topRightCornerRadius = (float)CornerRadius.TopRight * density;
			float bottomLeftCornerRadius = (float)CornerRadius.BottomLeft * density;
			float bottomRightCornerRadius = (float)CornerRadius.BottomRight * density;

			path.AppendRoundedRectangle(x, y, w, h, topLeftCornerRadius, topRightCornerRadius, bottomLeftCornerRadius, bottomRightCornerRadius);

			return path;
		}

		PathF IShape.PathForBounds(Graphics.Rect viewBounds)
		{
			_fallbackHeight = viewBounds.Height;
			_fallbackWidth = viewBounds.Width;

			var path = GetPath();

			TransformPathForBounds(path, viewBounds);

			return path;
		}

		internal PathF GetInnerPath(float strokeWidth)
		{
			var width = WidthForPathComputation;
			var height = HeightForPathComputation;

			var path = new PathF();

			float x = (float)StrokeThickness / 2;
			float y = (float)StrokeThickness / 2;

			float w = (float)(width - StrokeThickness);
			float h = (float)(height - StrokeThickness);

			float density = 1f;
#if ANDROID
			density = (float)DeviceDisplay.MainDisplayInfo.Density;
#endif
			float topLeftCornerRadius = (float)Math.Max(0, CornerRadius.TopLeft * density - strokeWidth);
			float topRightCornerRadius = (float)Math.Max(0, CornerRadius.TopRight * density - strokeWidth);
			float bottomLeftCornerRadius = (float)Math.Max(0, CornerRadius.BottomLeft * density - strokeWidth);
			float bottomRightCornerRadius = (float)Math.Max(0, CornerRadius.BottomRight * density - strokeWidth);

			path.AppendRoundedRectangle(x, y, w, h, topLeftCornerRadius, topRightCornerRadius, bottomLeftCornerRadius, bottomRightCornerRadius);

			return path;
		}

		PathF IRoundRectangle.InnerPathForBounds(Rect viewBounds, float strokeWidth)
		{
			_fallbackHeight = viewBounds.Height;
			_fallbackWidth = viewBounds.Width;

			var path = GetInnerPath(strokeWidth);

			TransformPathForBounds(path, viewBounds);

			return path;
		}
	}
}