using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
#pragma warning disable RS0016
	public class AccessibilityOverlayElement : IWindowOverlayElement
	{
		Maui.Graphics.Rect Rectangle { get; }
		string Message { get; }
		public Color FillColor { get; }
		public Color StrokeColor { get; }
		public Color TextColor { get; }

		bool _infoVisible;
		readonly Point _offset;

		public AccessibilityOverlayElement(Rect rect, string message, Maui.Graphics.Point? offset = null, 
			Color? fillColor = null, Color? strokeColor = null)
		{
			FillColor = fillColor ?? Colors.LightYellow.WithAlpha(0.6f);
			StrokeColor = strokeColor ?? Colors.Yellow;

			_offset = offset ?? Maui.Graphics.Point.Zero;

			rect = EnsureTouchable(rect);

			Rectangle = rect.Offset(_offset);
			Message = message;
			
			TextColor = Colors.White;
		}

		Rect EnsureTouchable(Rect rect) 
		{
			if (rect.Width < 44)
			{
				rect.Left = rect.Left - ((44 - rect.Width) / 2);
			}

			if (rect.Height < 44)
			{
				rect.Top = rect.Top - ((44 - rect.Height) / 2);
			}

			rect.Width = System.Math.Max(44, rect.Width);
			rect.Height = System.Math.Max(44, rect.Height);

			return rect;
		}

		public bool Contains(Graphics.Point point)
		{
			var offsetPoint = point.Offset(_offset.X, _offset.Y);
			return Rectangle.Contains(offsetPoint);
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			canvas.FillColor = FillColor;
			canvas.StrokeColor = StrokeColor;
			canvas.DrawRectangle(Rectangle);
			canvas.FillRectangle(Rectangle);

			canvas.FontColor = Colors.Red;
			canvas.FontSize = 20;
			canvas.DrawString("!", (float)Rectangle.Left, (float)Rectangle.Top + 5, HorizontalAlignment.Left);

			if (_infoVisible)
			{
				canvas.FontColor = TextColor;
				canvas.FontSize = 12;
				canvas.DrawString(Message, (float)Rectangle.Right + 5, (float)Rectangle.Top + 5, HorizontalAlignment.Left);
			}
		}

		public void ShowInfo() 
		{
			_infoVisible = true;
		}

		public void HideInfo() 
		{
			_infoVisible = false;
		}
	}
#pragma warning restore RS0016
}