using System;
using System.Collections.Generic;
using System.Numerics;

using Microsoft.Maui.Graphics.Text;

namespace Microsoft.Maui.Graphics
{
	public class PictureCanvas : ICanvas, IDisposable
	{
		private readonly float _x;
		private readonly float _y;
		private readonly float _width;
		private readonly float _height;
		private readonly List<DrawingCommand> _commands;

		public PictureCanvas(float x, float y, float width, float height)
		{
			_x = x;
			_y = y;
			_height = height;
			_width = width;

			_commands = new List<DrawingCommand>();
		}

		public IPicture Picture => new StandardPicture(_x, _y, _width, _height, _commands.ToArray());

		public void Dispose()
		{
			_commands.Clear();
		}

		public float DisplayScale { get; set; } = 1;

		public float StrokeSize
		{
			set
			{
				_commands.Add(
					canvas =>
						canvas.StrokeSize = value
				);
			}
		}

		public float MiterLimit
		{
			set { _commands.Add(canvas => canvas.MiterLimit = value); }
		}

		public Color StrokeColor
		{
			set { _commands.Add(canvas => canvas.StrokeColor = value); }
		}

		public LineCap StrokeLineCap
		{
			set { _commands.Add(canvas => canvas.StrokeLineCap = value); }
		}

		public LineJoin StrokeLineJoin
		{
			set { _commands.Add(canvas => canvas.StrokeLineJoin = value); }
		}

		public float[] StrokeDashPattern
		{
			set { _commands.Add(canvas => canvas.StrokeDashPattern = value); }
		}

		public float StrokeDashOffset
		{
			set { _commands.Add(canvas => canvas.StrokeDashOffset = value); }
		}

		public Color FillColor
		{
			set { _commands.Add(canvas => canvas.FillColor = value); }
		}

		public Color FontColor
		{
			set { _commands.Add(canvas => canvas.FontColor = value); }
		}

		public IFont Font
		{
			set { _commands.Add(canvas => canvas.Font = value); }
		}

		public float FontSize
		{
			set { _commands.Add(canvas => canvas.FontSize = value); }
		}

		public float Alpha
		{
			set { _commands.Add(canvas => canvas.Alpha = value); }
		}

		public bool Antialias
		{
			set
			{
				// Do nothing, not currently supported in a picture.
			}
		}

		public BlendMode BlendMode
		{
			set { _commands.Add(canvas => canvas.BlendMode = value); }
		}

		public void SubtractFromClip(float x, float y, float width, float height)
		{
			_commands.Add(canvas => canvas.SubtractFromClip(x, y, width, height));
		}

		public void DrawLine(float x1, float y1, float x2, float y2)
		{
			_commands.Add(
				canvas
					=> canvas.DrawLine(x1, y1, x2, y2)
			);
		}

		public void DrawArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed)
		{
			_commands.Add(canvas => canvas.DrawArc(x, y, width, height, startAngle, endAngle, clockwise, closed));
		}

		public void FillArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise)
		{
			_commands.Add(canvas => canvas.FillArc(x, y, width, height, startAngle, endAngle, clockwise));
		}

		public void DrawRectangle(float x, float y, float width, float height)
		{
			_commands.Add(canvas => canvas.DrawRectangle(x, y, width, height));
		}

		public void FillRectangle(float x, float y, float width, float height)
		{
			_commands.Add(canvas => canvas.FillRectangle(x, y, width, height));
		}

		public void DrawRoundedRectangle(float x, float y, float width, float height, float cornerRadius)
		{
			_commands.Add(canvas => canvas.DrawRoundedRectangle(x, y, width, height, cornerRadius));
		}

		public void FillRoundedRectangle(float x, float y, float width, float height, float cornerRadius)
		{
			_commands.Add(canvas => canvas.FillRoundedRectangle(x, y, width, height, cornerRadius));
		}

		public void DrawEllipse(float x, float y, float width, float height)
		{
			_commands.Add(canvas => canvas.DrawEllipse(x, y, width, height));
		}

		public void FillEllipse(float x, float y, float width, float height)
		{
			_commands.Add(canvas => canvas.FillEllipse(x, y, width, height));
		}

		public void DrawString(string value, float x, float y, HorizontalAlignment horizontalAlignment)
		{
			_commands.Add(canvas => canvas.DrawString(value, x, y, horizontalAlignment));
		}

		public void DrawString(string value, float x, float y, float width, float height, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment,
			TextFlow textFlow = TextFlow.ClipBounds, float lineSpacingAdjustment = 0)
		{
			_commands.Add(canvas => canvas.DrawString(value, x, y, width, height, horizontalAlignment, verticalAlignment, textFlow, lineSpacingAdjustment));
		}

		public void DrawText(IAttributedText value, float x, float y, float width, float height)
		{
			_commands.Add(canvas => canvas.DrawText(value, x, y, width, height));
		}

		public void DrawPath(PathF path)
		{
			_commands.Add(canvas => canvas.DrawPath(path));
		}

		public void FillPath(PathF path, WindingMode windingMode)
		{
			_commands.Add(canvas => canvas.FillPath(path, windingMode));
		}

		public void ClipPath(PathF path, WindingMode windingMode = WindingMode.NonZero)
		{
			_commands.Add(canvas => canvas.ClipPath(path, windingMode));
		}

		public void ClipRectangle(
			float x,
			float y,
			float width,
			float height)
		{
			_commands.Add(canvas => canvas.ClipRectangle(x, y, width, height));
		}

		public void Rotate(float degrees, float x, float y)
		{
			_commands.Add(canvas => canvas.Rotate(degrees, x, y));
		}

		public void Rotate(float degrees)
		{
			_commands.Add(canvas => canvas.Rotate(degrees));
		}

		public void Scale(float sx, float sy)
		{
			_commands.Add(canvas => canvas.Scale(sx, sy));
		}

		public void Translate(float tx, float ty)
		{
			_commands.Add(canvas => canvas.Translate(tx, ty));
		}

		public void ConcatenateTransform(Matrix3x2 transform)
		{
			_commands.Add(canvas => canvas.ConcatenateTransform(transform));
		}

		public void SaveState()
		{
			_commands.Add(canvas => canvas.SaveState());
		}

		public bool RestoreState()
		{
			_commands.Add(canvas => canvas.RestoreState());
			return true;
		}

		public void ResetState()
		{

		}

		public void SetShadow(SizeF offset, float blur, Color color)
		{
			_commands.Add(canvas => canvas.SetShadow(offset, blur, color));
		}

		public void SetFillPaint(Paint paint, PointF point1, PointF point2)
		{
			_commands.Add(canvas => canvas.SetFillPaint(paint, point1, point2));
		}

		public void SetFillPaint(Paint paint, RectF rectangle)
		{
			_commands.Add(canvas => canvas.SetFillPaint(paint, rectangle));
		}

		public SizeF GetStringSize(string value, IFont font, float fontSize)
		{
			throw new NotSupportedException();
		}

		public SizeF GetStringSize(string value, IFont font, float fontSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
		{
			throw new NotSupportedException();
		}

		public void DrawImage(IImage image, float x, float y, float width, float height)
		{
			_commands.Add(canvas => canvas.DrawImage(image, x, y, width, height));
		}
	}
}
