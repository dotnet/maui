#nullable enable
using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/BoxView.xml" path="Type[@FullName='Microsoft.Maui.Controls.BoxView']/Docs" />
	public partial class BoxView : IShapeView, IShape
	{
		protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == BackgroundColorProperty.PropertyName ||
				propertyName == ColorProperty.PropertyName ||
				propertyName == CornerRadiusProperty.PropertyName)
				Handler?.UpdateValue(nameof(IShapeView.Shape));
		}

		IShape? IShapeView.Shape => this;

		PathAspect IShapeView.Aspect => PathAspect.None;

		Paint? IShapeView.Fill => Color?.AsPaint() ?? ((IView)this).Background;

		Paint? IStroke.Stroke => null;

		double IStroke.StrokeThickness => 0;

		LineCap IStroke.StrokeLineCap => LineCap.Butt;

		LineJoin IStroke.StrokeLineJoin => LineJoin.Miter;

		float[]? IStroke.StrokeDashPattern => null;

		float IStroke.StrokeDashOffset => 0f;

		float IStroke.StrokeMiterLimit => 0;

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