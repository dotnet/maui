using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Text.Immutable;
using Microsoft.Maui.Graphics.Text.Mutable;

namespace Maui.Controls.Sample
{
	public class TestWindowOverlay : WindowOverlay
	{
		public TestWindowOverlay(IWindow window) 
			: base(window)
		{
			this.AddDrawable(new TestWindowDrawable());
		}
	}

	public class TestWindowDrawable : IDrawable
	{
		public void Draw(ICanvas canvas, RectangleF dirtyRect)
		{
			canvas.FillColor = Color.FromRgba(225, 225, 255, 225);
			canvas.StrokeColor = Color.FromRgba(225, 225, 255, 225);
			canvas.FillRectangle(0, 0, 300, 400);
		}
	}
}
