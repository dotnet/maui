using System;
using CoreGraphics;

namespace Microsoft.Maui.Graphics.Native
{
    public interface IGraphicsRenderer : IDisposable
    {
        NativeGraphicsView GraphicsView { set; }
        ICanvas Canvas { get; }
        IDrawable Drawable { get; set; }
        void Draw(CGContext coreGraphics, RectangleF dirtyRect);
        void SizeChanged(float width, float height);
        void Detached();
        void Invalidate();
        void Invalidate(float x, float y, float w, float h);
    }
}