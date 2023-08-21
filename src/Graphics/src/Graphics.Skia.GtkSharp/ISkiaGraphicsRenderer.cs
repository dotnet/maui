//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	public interface ISkiaGraphicsRenderer : IDisposable
	{
		GtkSkiaGraphicsView GraphicsView { set; }
		ICanvas Canvas { get; }
		IDrawable Drawable { get; set; }
		Color BackgroundColor { get; set; }
		void Draw(SKCanvas canvas, RectF dirtyRect);
		void SizeChanged(int width, int height);
		void Detached();
		void Invalidate();
		void Invalidate(float x, float y, float w, float h);
	}
}
