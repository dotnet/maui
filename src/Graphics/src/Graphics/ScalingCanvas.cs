using System;
using System.Collections.Generic;
using System.Numerics;

using Microsoft.Maui.Graphics.Text;

namespace Microsoft.Maui.Graphics
{
	public class ScalingCanvas : ICanvas, IBlurrableCanvas
	{
		private readonly ICanvas _canvas;
		private readonly IBlurrableCanvas _blurrableCanvas;
		private readonly Stack<float> _scaleXStack = new Stack<float>();
		private readonly Stack<float> _scaleYStack = new Stack<float>();
		private float _scaleX = 1f;
		private float _scaleY = 1f;

		public ScalingCanvas(ICanvas wrapped)
		{
			_canvas = wrapped;
			_blurrableCanvas = _canvas as IBlurrableCanvas;
		}

		public float DisplayScale
		{
			get => _canvas.DisplayScale;
			set => _canvas.DisplayScale = value;
		}

		public object Wrapped => _canvas;

		public ICanvas ParentCanvas => _canvas;

		public float StrokeSize
		{
			set => _canvas.StrokeSize = value;
		}

		public float MiterLimit
		{
			set => _canvas.MiterLimit = value;
		}

		public Color StrokeColor
		{
			set => _canvas.StrokeColor = value;
		}

		public LineCap StrokeLineCap
		{
			set => _canvas.StrokeLineCap = value;
		}

		public float Alpha
		{
			set => _canvas.Alpha = value;
		}

		public LineJoin StrokeLineJoin
		{
			set => _canvas.StrokeLineJoin = value;
		}

		public float[] StrokeDashPattern
		{
			set => _canvas.StrokeDashPattern = value;
		}

		public float StrokeDashOffset
		{
			set => _canvas.StrokeDashOffset = value;
		}

		public Color FillColor
		{
			set => _canvas.FillColor = value;
		}

		public Color FontColor
		{
			set => _canvas.FontColor = value;
		}

		public IFont Font
		{
			set => _canvas.Font = value;
		}

		public float FontSize
		{
			set => _canvas.FontSize = value;
		}

		public BlendMode BlendMode
		{
			set => _canvas.BlendMode = value;
		}

		public bool Antialias
		{
			set => _canvas.Antialias = value;
		}

		public void SubtractFromClip(float x1, float y1, float x2, float y2)
		{
			_canvas.SubtractFromClip(x1 * _scaleX, y1 * _scaleY, x2 * _scaleX, y2 * _scaleY);
		}

		public void DrawLine(float x1, float y1, float x2, float y2)
		{
			_canvas.DrawLine(x1 * _scaleX, y1 * _scaleY, x2 * _scaleX, y2 * _scaleY);
		}

		public void DrawArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed)
		{
			_canvas.DrawArc(x * _scaleX, y * _scaleY, width * _scaleX, height * _scaleY, startAngle, endAngle, clockwise, closed);
		}

		public void FillArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise)
		{
			_canvas.FillArc(x * _scaleX, y * _scaleY, width * _scaleX, height * _scaleY, startAngle, endAngle, clockwise);
		}

		public void DrawEllipse(float x, float y, float width, float height)
		{
			_canvas.DrawEllipse(x * _scaleX, y * _scaleY, width * _scaleX, height * _scaleY);
		}

		public void DrawImage(IImage image, float x, float y, float width, float height)
		{
			_canvas.DrawImage(image, x * _scaleX, y * _scaleY, width * _scaleX, height * _scaleY);
		}

		public void DrawRectangle(float x, float y, float width, float height)
		{
			_canvas.DrawRectangle(x * _scaleX, y * _scaleY, width * _scaleX, height * _scaleY);
		}

		public void DrawRoundedRectangle(float x, float y, float width, float height, float cornerRadius)
		{
			_canvas.DrawRoundedRectangle(x * _scaleX, y * _scaleY, width * _scaleX, height * _scaleY, cornerRadius * _scaleX);
		}

		public void DrawString(string value, float x, float y, HorizontalAlignment horizontalAlignment)
		{
			_canvas.DrawString(value, x * _scaleX, y * _scaleY, horizontalAlignment);
		}

		public void DrawString(string value, float x, float y, float width, float height, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment,
			TextFlow textFlow = TextFlow.ClipBounds, float lineSpacingAdjustment = 0)
		{
			_canvas.DrawString(value, x * _scaleX, y * _scaleY, width * _scaleX, height * _scaleY, horizontalAlignment, verticalAlignment, textFlow);
		}

		public void DrawText(IAttributedText value, float x, float y, float width, float height)
		{
			_canvas.DrawText(value, x * _scaleX, y * _scaleY, width * _scaleX, height * _scaleY);
		}

		public void FillEllipse(float x, float y, float width, float height)
		{
			_canvas.FillEllipse(x * _scaleX, y * _scaleY, width * _scaleX, height * _scaleY);
		}

		public void FillRectangle(float x, float y, float width, float height)
		{
			_canvas.FillRectangle(x * _scaleX, y * _scaleY, width * _scaleX, height * _scaleY);
		}

		public void FillRoundedRectangle(float x, float y, float width, float height, float cornerRadius)
		{
			_canvas.FillRoundedRectangle(x * _scaleX, y * _scaleY, width * _scaleX, height * _scaleY, cornerRadius * _scaleX);
		}

		public void DrawPath(PathF path)
		{
			var scaledPath = path.AsScaledPath(_scaleX, _scaleY);
			_canvas.DrawPath(scaledPath);
		}

		public void FillPath(PathF path, WindingMode windingMode)
		{
			var scaledPath = path.AsScaledPath(_scaleX, _scaleY);
			_canvas.FillPath(scaledPath, windingMode);
		}

		public void ClipPath(PathF path, WindingMode windingMode = WindingMode.NonZero)
		{
			var scaledPath = path.AsScaledPath(_scaleX, _scaleY);
			_canvas.ClipPath(scaledPath, windingMode);
		}

		public void ClipRectangle(float x, float y, float width, float height)
		{
			_canvas.ClipRectangle(x * _scaleX, y * _scaleY, width * _scaleX, height * _scaleY);
		}

		public void Rotate(float degrees, float x, float y)
		{
			_canvas.Rotate(degrees, x * _scaleX, y * _scaleY);
		}

		public void SetFillPaint(Paint paint, RectF rectangle)
		{
			_canvas.SetFillPaint(paint, new RectF(rectangle.X * _scaleX, rectangle.Y * _scaleY, rectangle.Width * _scaleX, rectangle.Height * _scaleY));
		}

		public void Rotate(float degrees)
		{
			_canvas.Rotate(degrees);
		}

		public void Scale(float sx, float sy)
		{
			_scaleX *= Math.Abs(sx);
			_scaleY *= Math.Abs(sy);
			_canvas.Scale(sx, sy);
		}

		public void Translate(float tx, float ty)
		{
			_canvas.Translate(tx, ty);
		}

		public void ConcatenateTransform(Matrix3x2 transform)
		{
			transform.DeconstructScales(out _, out var sx, out var sy);
			_scaleX *= sx;
			_scaleY *= sy;
			_canvas.ConcatenateTransform(transform);
		}

		public void SaveState()
		{
			_canvas.SaveState();
			_scaleXStack.Push(_scaleX);
			_scaleYStack.Push(_scaleY);
		}

		public void ResetState()
		{
			_canvas.ResetState();
			_scaleXStack.Clear();
			_scaleYStack.Clear();
			_scaleX = 1;
			_scaleY = 1;
		}

		public bool RestoreState()
		{
			var restored = _canvas.RestoreState();
			if (_scaleXStack.Count > 0)
			{
				_scaleX = _scaleXStack.Pop();
				_scaleY = _scaleYStack.Pop();
			}
			else
			{
				_scaleX = 1;
				_scaleY = 1;
			}

			return restored;
		}

		public float GetScale()
		{
			return _scaleX;
		}

		public void SetShadow(SizeF offset, float blur, Color color)
		{
			_canvas.SetShadow(offset, blur, color);
		}

		public void SetBlur(float blurRadius)
		{
			_blurrableCanvas?.SetBlur(blurRadius);
		}

		public SizeF GetStringSize(string value, IFont font, float fontSize)
			=> _canvas.GetStringSize(value, font, fontSize);

		public SizeF GetStringSize(string value, IFont font, float fontSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
			=> _canvas.GetStringSize(value, font, fontSize, horizontalAlignment, verticalAlignment);
	}
}
