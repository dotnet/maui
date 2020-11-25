using SkiaSharp;

namespace System.Graphics.Skia
{
    public static class SKGraphicsExtensions
    {
        public static SKColor AsSKColorMultiplyAlpha(this Color target, float alpha)
        {
            var r = (byte) (target.Red * 255f);
            var g = (byte) (target.Green * 255f);
            var b = (byte) (target.Blue * 255f);
            var a = (byte) (target.Alpha * alpha * 255f);

            if (a > 255)
                a = 255;

            var color = new SKColor(r, g, b, a);
            return color;
        }

        public static int ToArgb(this Color target)
        {
            var a = (int) (target.Alpha * 255f);
            var r = (int) (target.Red * 255f);
            var g = (int) (target.Green * 255f);
            var b = (int) (target.Blue * 255f);

            var argb = a << 24 | r << 16 | g << 8 | b;
            return argb;
        }

        public static int ToArgb(this Color target, float alpha)
        {
            var a = (int) (target.Alpha * 255f * alpha);
            var r = (int) (target.Red * 255f);
            var g = (int) (target.Green * 255f);
            var b = (int) (target.Blue * 255f);

            var argb = a << 24 | r << 16 | g << 8 | b;
            return argb;
        }

        public static SKColor AsSKColor(this Color target)
        {
            var r = (byte) (target.Red * 255f);
            var g = (byte) (target.Green * 255f);
            var b = (byte) (target.Blue * 255f);
            var a = (byte) (target.Alpha * 255f);
            return new SKColor(r, g, b, a);
        }

        public static Color AsColor(this SKColor target)
        {
            var r = (int) target.Red;
            var g = (int) target.Green;
            var b = (int) target.Blue;
            var a = (int) target.Alpha;
            return new Color(r, g, b, a);
        }

        public static SKRect AsSKRect(this RectangleF target)
        {
            return new SKRect(target.Left, target.Top, target.Right, target.Bottom);
        }

        public static RectangleF AsRectangleF(this SKRect target)
        {
            return new RectangleF(target.Left, target.Top, Math.Abs(target.Right - target.Left), Math.Abs(target.Bottom - target.Top));
        }

        public static SKPoint ToSKPoint(this PointF target)
        {
            return new SKPoint(target.X, target.Y);
        }

        /* public static EWPath AsEWPath(this global::Android.Graphics.Path target)
        {
            var vPath = new EWPath();
  
            var vConverter = new PathConverter(vPath);
            target.Apply(vConverter.ApplyCGPathFunction);
  
            return vPath;
        } */

        public static SKMatrix AsMatrix(this AffineTransform transform)
        {
            var matrix = new SKMatrix
            {
                ScaleX = transform.ScaleX,
                SkewX = transform.ShearX,
                TransX = transform.TranslateX,
                SkewY = transform.ShearY,
                ScaleY = transform.ScaleY,
                TransY = transform.TranslateY,
                Persp0 = 0,
                Persp1 = 0,
                Persp2 = 1
            };
            return matrix;
        }

        public static SKPath AsSkiaPath(this PathF target)
        {
            return AsSkiaPath(target, 1);
        }

        public static SKPath AsSkiaPath(this PathF path, float ppu)
        {
            return AsSkiaPath(path, ppu, 0, 0, 1, 1);
        }

        public static SKPath AsSkiaPath(
            this PathF path,
            float ppu,
            float ox,
            float oy,
            float fx,
            float fy)
        {
            var nativePath = new SKPath();

            var ppux = ppu * fx;
            var ppuy = ppu * fy;

            var pointIndex = 0;
            var arcAngleIndex = 0;
            var arcClockwiseIndex = 0;

            foreach (var type in path.SegmentTypes)
            {
                if (type == PathOperation.Move)
                {
                    var point = path[pointIndex++];
                    nativePath.MoveTo((ox + point.X * ppux), (oy + point.Y * ppuy));
                }
                else if (type == PathOperation.Line)
                {
                    var point = path[pointIndex++];
                    nativePath.LineTo((ox + point.X * ppux), (oy + point.Y * ppuy));
                }

                else if (type == PathOperation.Quad)
                {
                    var controlPoint = path[pointIndex++];
                    var point = path[pointIndex++];
                    nativePath.QuadTo((ox + controlPoint.X * ppux), (oy + controlPoint.Y * ppuy), (ox + point.X * ppux), (oy + point.Y * ppuy));
                }
                else if (type == PathOperation.Cubic)
                {
                    var controlPoint1 = path[pointIndex++];
                    var controlPoint2 = path[pointIndex++];
                    var point = path[pointIndex++];
                    nativePath.CubicTo((ox + controlPoint1.X * ppux), (oy + controlPoint1.Y * ppuy), (ox + controlPoint2.X * ppux), (oy + controlPoint2.Y * ppuy), (ox + point.X * ppux),
                        (oy + point.Y * ppuy));
                }
                else if (type == PathOperation.Arc)
                {
                    var topLeft = path[pointIndex++];
                    var bottomRight = path[pointIndex++];
                    var startAngle = path.GetArcAngle(arcAngleIndex++);
                    var endAngle = path.GetArcAngle(arcAngleIndex++);
                    var clockwise = path.GetArcClockwise(arcClockwiseIndex++);

                    while (startAngle < 0)
                    {
                        startAngle += 360;
                    }

                    while (endAngle < 0)
                    {
                        endAngle += 360;
                    }

                    var rect = new SKRect(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
                    var sweep = Geometry.GetSweep(startAngle, endAngle, clockwise);
                    
                    startAngle *= -1;
                    if (!clockwise)
                        sweep *= -1;

                    nativePath.AddArc(rect, startAngle, sweep);
                }
                else if (type == PathOperation.Close)
                {
                    nativePath.Close();
                }
            }

            return nativePath;
        }

        public static SKPath AsSkiaPath(this PathF path, float ppu, float zoom)
        {
            return AsSkiaPath(path, ppu * zoom);
        }

        public static SKPath AsSkiaPathFromSegment(this PathF target, int segmentIndex, float ppu, float zoom)
        {
            ppu = zoom * ppu;

            var path = new SKPath();

            var type = target.GetSegmentType(segmentIndex);
            if (type == PathOperation.Line)
            {
                var pointIndex = target.GetSegmentPointIndex(segmentIndex);
                var startPoint = target[pointIndex - 1];
                path.MoveTo(startPoint.X * ppu, startPoint.Y * ppu);

                var endPoint = target[pointIndex];
                path.LineTo(endPoint.X * ppu, endPoint.Y * ppu);
            }
            else if (type == PathOperation.Quad)
            {
                var pointIndex = target.GetSegmentPointIndex(segmentIndex);
                var startPoint = target[pointIndex - 1];
                path.MoveTo(startPoint.X * ppu, startPoint.Y * ppu);

                var controlPoint = target[pointIndex++];
                var endPoint = target[pointIndex];
                path.QuadTo(controlPoint.X * ppu, controlPoint.Y * ppu, endPoint.X * ppu, endPoint.Y * ppu);
            }
            else if (type == PathOperation.Cubic)
            {
                var pointIndex = target.GetSegmentPointIndex(segmentIndex);
                var startPoint = target[pointIndex - 1];
                path.MoveTo(startPoint.X * ppu, startPoint.Y * ppu);

                var controlPoint1 = target[pointIndex++];
                var controlPoint2 = target[pointIndex++];
                var endPoint = target[pointIndex];
                path.CubicTo(controlPoint1.X * ppu, controlPoint1.Y * ppu, controlPoint2.X * ppu, controlPoint2.Y * ppu, endPoint.X * ppu, endPoint.Y * ppu);
            }
            else if (type == PathOperation.Arc)
            {
                target.GetSegmentInfo(segmentIndex, out var pointIndex, out var arcAngleIndex, out var arcClockwiseIndex);

                var topLeft = target[pointIndex++];
                var bottomRight = target[pointIndex];
                var startAngle = target.GetArcAngle(arcAngleIndex++);
                var endAngle = target.GetArcAngle(arcAngleIndex);
                var clockwise = target.GetArcClockwise(arcClockwiseIndex);

                while (startAngle < 0)
                {
                    startAngle += 360;
                }

                while (endAngle < 0)
                {
                    endAngle += 360;
                }

                var rect = new SKRect(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
                var sweep = Geometry.GetSweep(startAngle, endAngle, clockwise);

                startAngle *= -1;
                if (!clockwise)
                    sweep *= -1;

                path.AddArc(rect, startAngle, sweep);
            }

            return path;
        }

        public static SKPath AsRotatedAndroidPath(this PathF target, PointF center, float ppu, float zoom, float angle)
        {
            ppu = zoom * ppu;

            var path = new SKPath();

            var pointIndex = 0;
            var arcAngleIndex = 0;
            var arcClockwiseIndex = 0;

            foreach (var type in target.SegmentTypes)
            {
                if (type == PathOperation.Move)
                {
                    var point = target.GetRotatedPoint(pointIndex++, center, angle);
                    path.MoveTo(point.X * ppu, point.Y * ppu);
                }
                else if (type == PathOperation.Line)
                {
                    var endPoint = target.GetRotatedPoint(pointIndex++, center, angle);
                    path.LineTo(endPoint.X * ppu, endPoint.Y * ppu);
                }
                else if (type == PathOperation.Quad)
                {
                    var controlPoint1 = target.GetRotatedPoint(pointIndex++, center, angle);
                    var endPoint = target.GetRotatedPoint(pointIndex++, center, angle);
                    path.QuadTo(
                        controlPoint1.X * ppu,
                        controlPoint1.Y * ppu,
                        endPoint.X * ppu,
                        endPoint.Y * ppu);
                }
                else if (type == PathOperation.Cubic)
                {
                    var controlPoint1 = target.GetRotatedPoint(pointIndex++, center, angle);
                    var controlPoint2 = target.GetRotatedPoint(pointIndex++, center, angle);
                    var endPoint = target.GetRotatedPoint(pointIndex++, center, angle);
                    path.CubicTo(
                        controlPoint1.X * ppu,
                        controlPoint1.Y * ppu,
                        controlPoint2.X * ppu,
                        controlPoint2.Y * ppu,
                        endPoint.X * ppu,
                        endPoint.Y * ppu);
                }
                else if (type == PathOperation.Arc)
                {
                    var topLeft = target[pointIndex++];
                    var bottomRight = target[pointIndex++];
                    var startAngle = target.GetArcAngle(arcAngleIndex++);
                    var endAngle = target.GetArcAngle(arcAngleIndex++);
                    var clockwise = target.GetArcClockwise(arcClockwiseIndex++);

                    while (startAngle < 0)
                    {
                        startAngle += 360;
                    }

                    while (endAngle < 0)
                    {
                        endAngle += 360;
                    }

                    var rect = new SKRect(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
                    var sweep = Geometry.GetSweep(startAngle, endAngle, clockwise);

                    startAngle *= -1;
                    if (!clockwise)
                        sweep *= -1;

                    path.AddArc(rect, startAngle, sweep);
                }
                else if (type == PathOperation.Close)
                {
                    path.Close();
                }
            }

            return path;
        }

        public static SizeF AsSize(this SKSize target)
        {
            return new SizeF(target.Width, target.Height);
        }

        public static SKSize AsSizeF(this SizeF target)
        {
            return new SKSize(target.Width, target.Height);
        }

        public static PointF AsEWPoint(this SKPoint target)
        {
            return new PointF(target.X, target.Y);
        }

        public static SKBitmap GetPatternBitmap(this Paint paint, float scale = 1)
        {
            var pattern = paint?.Pattern;
            if (pattern == null)
                return null;

            using (var context = new SkiaBitmapExportContext((int) (pattern.Width * scale), (int) (pattern.Height * scale), scale, disposeBitmap: false))
            {
                var canvas = context.Canvas;

                canvas.Scale(scale, scale);
                pattern.Draw(canvas);

                return context.Bitmap;
            }
        }

        public static SKBitmap GetPatternBitmap(this Paint paint, float scaleX, float scaleY, object currentFigure)
        {
            var pattern = paint?.Pattern;
            if (pattern == null)
                return null;

            using (var context = new SkiaBitmapExportContext((int) (pattern.Width * scaleX), (int) (pattern.Height * scaleY), 1, disposeBitmap: false))
            {
                var canvas = context.Canvas;

                if (currentFigure != null)
                {
                }

                canvas.Scale(scaleX, scaleY);
                pattern.Draw(canvas);

                if (currentFigure != null)
                {
                }

                //var filename = "/storage/emulated/0/" + pattern.GetType().Name + ".png";
                //System.Console.WriteLine("Writing to :{0}",filename);
                //context.WriteToFile (filename);
                return context.Bitmap;
            }
        }

        /*
        public static EWSize GetTextSizeAsEWSize(this StaticLayout target, bool hasBoundedWidth)
        {
            // We need to know if the static layout was created with a bounded width, as this is what 
            // StaticLayout.Width returns.
            if (hasBoundedWidth)
                return new EWSize(target.Width, target.Height);
  
            float maxWidth = 0;
            int lineCount = target.LineCount;
  
            for (int i = 0; i < lineCount; i++)
            {
                float lineWidth = target.GetLineWidth(i);
                if (lineWidth > maxWidth)
                {
                    maxWidth = lineWidth;
                }
            }
  
            return new EWSize(maxWidth, target.Height);
        }*/

        /*
        public static SKSize GetOffsetsToDrawText(
            this StaticLayout target, 
            float x, 
            float y, 
            float width, 
            float height, 
            float margin, 
            EWHorizontalAlignment horizontalAlignment, 
            EWVerticalAlignment verticalAlignment)
        {
            if (verticalAlignment != EWVerticalAlignment.TOP)
            {
                SizeF vTextFrameSize = target.GetTextSize();
  
                float vOffsetX = margin;
                float vOffsetY = margin;
  
                if (height > 0)
                {
                    if (verticalAlignment == EWVerticalAlignment.BOTTOM)
                        vOffsetY = height - vTextFrameSize.Height - margin;
                    else
                        vOffsetY = (height - vTextFrameSize.Height)/2;
                }
  
                return new SizeF(x + vOffsetX, y + vOffsetY);
            }
  
            return new SizeF(x + margin, y + margin);
        }*/

        /*
        public static Bitmap Downsize(this Bitmap target, int maxSize, bool dispose = true)
        {
            if (target.Width > maxSize || target.Height > maxSize)
            {
                float factor = 1;
  
                if (target.Width > target.Height)
                {
                    factor = (float)maxSize / (float)target.Width;   
                }
                else
                {
                    factor = (float)maxSize / (float)target.Height;                      
                }
  
                var w = (int)Math.Round(factor * (float)target.Width);
                var h = (int)Math.Round(factor * (float)target.Height);
  
                var newImage = Bitmap.CreateScaledBitmap(target, w, h, true);
                if (dispose) 
                {
                    target.Recycle ();
                    target.Dispose ();
                }
                return newImage;
            }
  
            return target;
        }*/

        /*
        public static Bitmap Downsize(this Bitmap target, int maxWidth, int maxHeight, bool dispose = true)
        {
            var newImage = Bitmap.CreateScaledBitmap(target, maxWidth, maxHeight, true);
            if (dispose) 
            {
                target.Recycle ();
                target.Dispose ();
            }
            return newImage;
        }*/
    }
}