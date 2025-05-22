using System;
using System.Collections.Generic;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// Provides functionality for laying out text using SkiaSharp.
	/// </summary>
	public class SkiaTextLayout : IDisposable
	{
		private readonly LayoutLine _callback;
		private readonly RectF _rect;
		private readonly ITextAttributes _textAttributes;
		private readonly string _value;
		private readonly TextFlow _textFlow;
		private readonly SKFont _font;
		private readonly bool _disposeFont;
		private readonly float _lineHeight;
		private readonly float _descent;

		/// <summary>
		/// Gets or sets a value indicating whether text should wrap to the next line when it exceeds the layout width.
		/// </summary>
		public bool WordWrap { get; set; } = true;

		/// <summary>
		/// Initializes a new instance of the <see cref="SkiaTextLayout"/> class.
		/// </summary>
		/// <param name="value">The text to layout.</param>
		/// <param name="rect">The rectangle in which to layout the text.</param>
		/// <param name="textAttributes">The text attributes to apply.</param>
		/// <param name="callback">The callback to invoke for each line of text.</param>
		/// <param name="textFlow">The text flow behavior.</param>
		/// <param name="paint">The SkiaSharp paint object to use. This method is obsolete.</param>
		[Obsolete("Use SkiaTextLayout(string, RectF, ITextAttributes, LayoutLine, TextFlow, SKFont) instead.")]
		public SkiaTextLayout(
			string value,
			RectF rect,
			ITextAttributes textAttributes,
			LayoutLine callback,
			TextFlow textFlow = TextFlow.ClipBounds,
			SKPaint paint = null)
			: this(value, rect, textAttributes, callback, textFlow, paint?.ToFont())
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SkiaTextLayout"/> class.
		/// </summary>
		/// <param name="value">The text to layout.</param>
		/// <param name="rect">The rectangle in which to layout the text.</param>
		/// <param name="textAttributes">The text attributes to apply.</param>
		/// <param name="callback">The callback to invoke for each line of text.</param>
		/// <param name="textFlow">The text flow behavior.</param>
		/// <param name="font">The SkiaSharp font object to use.</param>
		public SkiaTextLayout(
			string value,
			RectF rect,
			ITextAttributes textAttributes,
			LayoutLine callback,
			TextFlow textFlow = TextFlow.ClipBounds,
			SKFont font = null)
		{
			_value = value;
			_textAttributes = textAttributes;
			_rect = rect;
			_callback = callback;
			_textFlow = textFlow;

			if (font is not null)
			{
				_font = font;
			}
			else
			{
				_font = new SKFont()
				{
					Typeface = _textAttributes.Font?.ToSKTypeface() ?? SKTypeface.Default,
					Size = _textAttributes.FontSize
				};

				_disposeFont = true;
			}

			var metrics = _font.Metrics;
			_descent = metrics.Descent;
			_lineHeight = _font.Spacing;
		}

		/// <summary>
		/// Performs layout of text within the specified rectangle.
		/// </summary>
		public void LayoutText()
		{
			if (string.IsNullOrEmpty(_value))
				return;

			var x = _rect.X;
			var y = _rect.Y;
			var width = _rect.Width;
			var height = _rect.Height;

			x += _textAttributes.Margin;
			y += _textAttributes.Margin;
			width -= (_textAttributes.Margin * 2);
			height -= (_textAttributes.Margin * 2);

			var top = y;
			var bottom = y + height;

			var lines = CreateLines(y, bottom, width);
			switch (_textAttributes.VerticalAlignment)
			{
				case VerticalAlignment.Center:
					LayoutCenterAligned(lines, x, width, top, height);
					break;
				case VerticalAlignment.Bottom:
					LayoutBottomAligned(lines, x, width, bottom, top);
					break;
				default:
					LayoutTopAligned(lines, x, y, width);
					break;
			}
		}

		private void LayoutCenterAligned(
			List<TextLine> lines,
			float x,
			float width,
			float top,
			float height)
		{
			var linesToDraw = lines.Count;

			if (_textFlow == TextFlow.ClipBounds)
			{
				var maxLines = Math.Floor(height / _lineHeight);
				linesToDraw = (int)Math.Min(maxLines, lines.Count);
			}

			// Figure out the vertical center of the rect
			var y = top + height / 2;

			// Figure out the center index of the list, and the center point to start drawing from.
			var startIndex = (lines.Count / 2);
			if (linesToDraw % 2 != 0)
				y -= _lineHeight / 2;

			// Figure out which index to draw first (of the range) and the point of the first line.
			for (var i = 0; i < linesToDraw / 2; i++)
			{
				y -= _lineHeight;
				startIndex--;
			}

			y -= _descent;

			// Draw each line.
			for (var i = 0; i < linesToDraw; i++)
			{
				y += _lineHeight;
				var line = lines[i + startIndex];

				var point = new PointF(x, y);
				switch (_textAttributes.HorizontalAlignment)
				{
					case HorizontalAlignment.Center:
						point.X = x + width / 2;
						break;
					case HorizontalAlignment.Right:
						point.X = x + width;
						break;
				}

				_callback(point, _textAttributes, line.Value, 0, 0, 0);
			}
		}

		private void LayoutBottomAligned(
			List<TextLine> lines,
			float x,
			float width,
			float bottom,
			float top)
		{
			var y = bottom - _descent;

			for (int i = lines.Count - 1; i >= 0; i--)
			{
				var line = lines[i];

				if (_textFlow == TextFlow.ClipBounds && y - _lineHeight < top)
					return;

				var point = new PointF(x, y);
				switch (_textAttributes.HorizontalAlignment)
				{
					case HorizontalAlignment.Center:
						point.X = x + width / 2;
						break;
					case HorizontalAlignment.Right:
						point.X = x + width;
						break;
				}

				_callback(point, _textAttributes, line.Value, 0, 0, 0);

				y -= _lineHeight;
			}
		}

		private void LayoutTopAligned(
			List<TextLine> lines,
			float x,
			float y,
			float width)
		{
			y -= _descent;

			foreach (var line in lines)
			{
				y += _lineHeight;

				var point = new PointF(x, y);
				switch (_textAttributes.HorizontalAlignment)
				{
					case HorizontalAlignment.Center:
						point.X = x + width / 2;
						break;
					case HorizontalAlignment.Right:
						point.X = x + width;
						break;
				}

				_callback(point, _textAttributes, line.Value, 0, 0, 0);
			}
		}

		private List<TextLine> CreateLines(float y, float bottom, float width)
		{
			var lines = new List<TextLine>();

			var index = 0;
			var length = _value.Length;
			while (index < length)
			{
				y += _lineHeight;

				if (_textFlow == TextFlow.ClipBounds && _textAttributes.VerticalAlignment == VerticalAlignment.Top && y > bottom)
					return lines;

				var count = _font.BreakText(_value.AsSpan(index), width, out var textWidth);

				var found = false;
				if (WordWrap && index + count < length)
				{
					for (var i = index + count - 1; i >= index && !found; i--)
					{
						if (char.IsWhiteSpace(_value[i]))
						{
							count = i - index + 1;
							found = true;
						}
					}
				}

				var line = _value.AsSpan(index, count);
				if (found)
				{
					textWidth = _font.MeasureText(line);
				}
				lines.Add(new TextLine(line.ToString(), textWidth));

				index += count;
			}

			return lines;
		}

		/// <summary>
		/// Releases all resources used by this text layout.
		/// </summary>
		public void Dispose()
		{
			if (_disposeFont)
				_font?.Dispose();
		}
	}

	/// <summary>
	/// Represents a line of text with its measured width.
	/// </summary>
	/// <param name="value">The text content of the line.</param>
	/// <param name="width">The measured width of the line.</param>
	public class TextLine(string value, float width)
	{
		/// <summary>
		/// Gets the text content of the line.
		/// </summary>
		public string Value { get; } = value;

		/// <summary>
		/// Gets the measured width of the line.
		/// </summary>
		public float Width { get; } = width;
	}
}
