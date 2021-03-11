using Microsoft.Maui.Graphics.Blazor.Canvas2D;

namespace Microsoft.Maui.Graphics.Blazor
{
    public static class BlazorCanvasExtensions
    {
        public static Canvas2D.LineCap AsCanvasValue(
            this LineCap target)
        {
            switch (target)
            {
                case LineCap.Butt:
                    return Canvas2D.LineCap.Butt;
                case LineCap.Round:
                    return Canvas2D.LineCap.Round;
                case LineCap.Square:
                    return Canvas2D.LineCap.Square;
            }

            return Canvas2D.LineCap.Butt;
        }

        public static Canvas2D.LineJoin AsCanvasValue(
            this LineJoin target)
        {
            switch (target)
            {
                case LineJoin.Miter:
                    return Canvas2D.LineJoin.Miter;
                case LineJoin.Round:
                    return Canvas2D.LineJoin.Round;
                case LineJoin.Bevel:
                    return Canvas2D.LineJoin.Bevel;
            }

            return Canvas2D.LineJoin.Miter;
        }

        public static string AsCanvasValue(
            this Color color,
            string defaultValue = "black")
        {
            if (color != null)
                return color.ToHexString();

            return defaultValue;
        }
    }
}
