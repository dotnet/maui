using System.Numerics;
using Microsoft.Maui.Graphics.Text;

namespace Microsoft.Maui.Graphics
{
	public interface ICanvas
	{
		public float DisplayScale { get; set; }
		public float StrokeSize { set; }
		public float MiterLimit { set; }
		public Color StrokeColor { set; }
		public LineCap StrokeLineCap { set; }
		public LineJoin StrokeLineJoin { set; }
		public float[] StrokeDashPattern { set; }
		public float StrokeDashOffset { set; }
		public Color FillColor { set; }
		public Color FontColor { set; }
		public IFont Font { set; }
		public float FontSize { set; }
		public float Alpha { set; }
		public bool Antialias { set; }
		public BlendMode BlendMode { set; }

		public void DrawPath(PathF path);

		public void FillPath(PathF path, WindingMode windingMode);

		public void SubtractFromClip(float x, float y, float width, float height);

		public void ClipPath(PathF path, WindingMode windingMode = WindingMode.NonZero);

		public void ClipRectangle(float x, float y, float width, float height);

		public void DrawLine(float x1, float y1, float x2, float y2);

		public void DrawArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed);

		public void FillArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise);

		public void DrawRectangle(float x, float y, float width, float height);

		public void FillRectangle(float x, float y, float width, float height);

		public void DrawRoundedRectangle(float x, float y, float width, float height, float cornerRadius);

		public void FillRoundedRectangle(float x, float y, float width, float height, float cornerRadius);

		public void DrawEllipse(float x, float y, float width, float height);

		public void FillEllipse(float x, float y, float width, float height);

		public void DrawString(string value, float x, float y, HorizontalAlignment horizontalAlignment);

		public void DrawString(
			string value,
			float x,
			float y,
			float width,
			float height,
			HorizontalAlignment horizontalAlignment,
			VerticalAlignment verticalAlignment,
			TextFlow textFlow = TextFlow.ClipBounds,
			float lineSpacingAdjustment = 0);

		public void DrawText(
			IAttributedText value,
			float x,
			float y,
			float width,
			float height);

		public void Rotate(float degrees, float x, float y);

		public void Rotate(float degrees);

		public void Scale(float sx, float sy);

		public void Translate(float tx, float ty);

		public void ConcatenateTransform(Matrix3x2 transform);

		public void SaveState();

		public bool RestoreState();

		public void ResetState();

		public void SetShadow(SizeF offset, float blur, Color color);

		public void SetFillPaint(Paint paint, RectF rectangle);

		public void DrawImage(IImage image, float x, float y, float width, float height);

		public SizeF GetStringSize(string value, IFont font, float fontSize);

		public SizeF GetStringSize(string value, IFont font, float fontSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment);
	}
}
