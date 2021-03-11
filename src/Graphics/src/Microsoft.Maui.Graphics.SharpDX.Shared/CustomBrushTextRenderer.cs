using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using System;

namespace Microsoft.Maui.Graphics.SharpDX
{
    public class CustomBrushTextRenderer : TextRendererBase
    {
        private readonly RenderTarget _renderTarget;
        private readonly Brush _defaultBrush;
        private readonly bool _isPixelSnappingDisabled;

        public CustomBrushTextRenderer(
            RenderTarget renderTarget,
            Brush defaultBrush,
            bool isPixelSnappingDisabled)
        {
            _renderTarget = renderTarget;
            _defaultBrush = defaultBrush;
            _isPixelSnappingDisabled = isPixelSnappingDisabled;
        }

        public override Result DrawGlyphRun(
            object clientDrawingContext,
            float baselineOriginX,
            float baselineOriginY,
            MeasuringMode measuringMode,
            GlyphRun glyphRun,
            GlyphRunDescription glyphRunDescription,
            ComObject clientDrawingEffect)
        {
            var brush = clientDrawingEffect as Brush ?? _defaultBrush;
            if (brush == null) throw new NullReferenceException("No brush is set as a drawing effect for this glyph run and no default brush was given.");

            var textBrush = brush as TextBrush;
            SolidColorBrush backgroundBrush = textBrush?.Background;
            if (backgroundBrush != null)
            {
                // Get width of text
                float totalWidth = 0;

                foreach (float advance in glyphRun.Advances)
                    totalWidth += advance;

                // Get height of text
                var fontMetrics = glyphRun.FontFace.Metrics;
                var adjust = glyphRun.FontSize / fontMetrics.DesignUnitsPerEm;
                var ascent = adjust * fontMetrics.Ascent;
                var descent = adjust * fontMetrics.Descent;
                var rect = new RawRectangleF(baselineOriginX,
                    baselineOriginY - ascent,
                    baselineOriginX + totalWidth,
                    baselineOriginY + descent);

                // Fill Rectangle
                _renderTarget.FillRectangle(rect, backgroundBrush);
            }

            _renderTarget.DrawGlyphRun(new Vector2(baselineOriginX, baselineOriginY), glyphRun, brush, measuringMode);
            return Result.Ok;
        }

        public override Result DrawStrikethrough(
            object clientDrawingContext,
            float baselineOriginX,
            float baselineOriginY,
            ref Strikethrough strikethrough,
            ComObject clientDrawingEffect)
        {
            var point0 = new Vector2(baselineOriginX, baselineOriginY + strikethrough.Offset);
            var point1 = new Vector2(baselineOriginX + strikethrough.Width, baselineOriginY + strikethrough.Offset);
            var brush = clientDrawingEffect as Brush ?? _defaultBrush;
            _renderTarget.DrawLine(point0, point1, brush, strikethrough.Thickness);
            return Result.Ok;
        }


        public override Result DrawUnderline(
            object clientDrawingContext,
            float baselineOriginX,
            float baselineOriginY,
            ref Underline underline,
            ComObject clientDrawingEffect)
        {
            var point0 = new Vector2(baselineOriginX, baselineOriginY + underline.Offset);
            var point1 = new Vector2(baselineOriginX + underline.Width, baselineOriginY + underline.Offset);
            var brush = clientDrawingEffect as Brush ?? _defaultBrush;
            _renderTarget.DrawLine(point0, point1, brush, underline.Thickness);
            return Result.Ok;
        }

        public override bool IsPixelSnappingDisabled(object clientDrawingContext)
        {
            return _isPixelSnappingDisabled;
        }

        public static void DrawTextLayout(
            RenderTarget renderTarget,
            Vector2 origin,
            TextLayout textLayout,
            Brush defaultForegroundBrush,
            DrawTextOptions options = DrawTextOptions.None)
        {
            if (options.HasFlag(DrawTextOptions.Clip))
            {
                renderTarget.PushAxisAlignedClip(new global::SharpDX.RectangleF(origin.X, origin.Y, textLayout.MaxWidth, textLayout.MaxHeight), renderTarget.AntialiasMode);
                using (var renderer = new CustomBrushTextRenderer(renderTarget, defaultForegroundBrush, options.HasFlag(DrawTextOptions.NoSnap)))
                    textLayout.Draw(renderer, origin.X, origin.Y);
                renderTarget.PopAxisAlignedClip();
            }
            else
            {
                using (var renderer = new CustomBrushTextRenderer(renderTarget, defaultForegroundBrush, options.HasFlag(DrawTextOptions.NoSnap)))
                    textLayout.Draw(renderer, origin.X, origin.Y);
            }
        }
    }
}