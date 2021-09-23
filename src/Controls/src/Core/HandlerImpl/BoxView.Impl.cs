#nullable enable
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public partial class BoxView : IShapeView, IShape
	{
		protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == ColorProperty.PropertyName ||
				propertyName == CornerRadiusProperty.PropertyName)
				Handler?.UpdateValue(nameof(IShapeView.Shape));
		}

		IShape? IShapeView.Shape => this;

		PathAspect IShapeView.Aspect => PathAspect.None;

		Paint? IShapeView.Fill => Color?.AsPaint() ?? ((IView)this).Background;

		Paint? IShapeView.Stroke => null;

		double IShapeView.StrokeThickness => 0;

		LineCap IShapeView.StrokeLineCap => LineCap.Butt;

		LineJoin IShapeView.StrokeLineJoin => LineJoin.Miter;

		float[]? IShapeView.StrokeDashPattern => null;

		float IShapeView.StrokeMiterLimit => 0;

		PathF IShape.PathForBounds(Rectangle bounds)
		{
			var path = new PathF();

			path.AppendRoundedRectangle(
				bounds,
				(float)CornerRadius.TopLeft,
				(float)CornerRadius.TopRight,
				(float)CornerRadius.BottomLeft,
				(float)CornerRadius.BottomRight);

			return path;
		}
	}
}