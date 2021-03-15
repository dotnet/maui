using Microsoft.Maui.Graphics.Blazor.Canvas2D;

namespace Microsoft.Maui.Graphics.Blazor
{
    public static class ComponentExtensions
    {
        public static CanvasRenderingContext2D CreateRenderingContext2D(this CanvasComponentBase canvas)
        {
            return new CanvasRenderingContext2D(canvas);
        }
    }
}
