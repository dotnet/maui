using System;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Graphics.Drawables.Shapes;
using AColor = Android.Graphics.Color;
using AMatrix = Android.Graphics.Matrix;
using APath = Android.Graphics.Path;
using AView = Android.Views.View;
using Shape = Xamarin.Forms.Shapes.Shape;

namespace Xamarin.Forms.Platform.Android
{
    public class ShapeRenderer<TShape, TNativeShape> : ViewRenderer<TShape, TNativeShape>
         where TShape : Shape
         where TNativeShape : ShapeView
    {
        double _height;
        double _width;

        public ShapeRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<TShape> args)
        {
            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                UpdateSize();
                UpdateAspect();
                UpdateFill();
                UpdateStroke();
                UpdateStrokeThickness();
                UpdateStrokeDashArray();
                UpdateStrokeLineCap();
                UpdateStrokeLineJoin();
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(sender, args);

            if (args.PropertyName == VisualElement.HeightProperty.PropertyName)
            {
                _height = Element.Height;
                UpdateSize();
            }
            else if (args.PropertyName == VisualElement.WidthProperty.PropertyName)
            {
                _width = Element.Width;
                UpdateSize();
            }
            else if (args.PropertyName == Shape.AspectProperty.PropertyName)
                UpdateAspect();
            else if (args.PropertyName == Shape.FillProperty.PropertyName)
                UpdateFill();
            else if (args.PropertyName == Shape.StrokeProperty.PropertyName)
                UpdateStroke();
            else if (args.PropertyName == Shape.StrokeThicknessProperty.PropertyName)
                UpdateStrokeThickness();
            else if (args.PropertyName == Shape.StrokeDashArrayProperty.PropertyName)
                UpdateStrokeDashArray();
            else if (args.PropertyName == Shape.StrokeLineCapProperty.PropertyName)
                UpdateStrokeLineCap();
            else if (args.PropertyName == Shape.StrokeLineJoinProperty.PropertyName)
                UpdateStrokeLineJoin();
        }

        public override SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
        {
            if (Element != null)
            {
                return Control.GetDesiredSize();
            }

            return base.GetDesiredSize(widthConstraint, heightConstraint);
        }

        void UpdateSize()
        {
            Control.UpdateSize(_width, _height);
        }

        void UpdateAspect()
        {
            Control.UpdateAspect(Element.Aspect);
        }

        void UpdateFill()
        {
            Control.UpdateFill(Element.Fill.ToAndroid());
        }

        void UpdateStroke()
        {
            Control.UpdateStroke(Element.Stroke.ToAndroid());
        }

        void UpdateStrokeThickness()
        {
            Control.UpdateStrokeThickness((float)Element.StrokeThickness);
        }

        void UpdateStrokeDashArray()
        {
            Control.UpdateStrokeDashArray(Element.StrokeDashArray.ToArray());
        }

        void UpdateStrokeLineCap()
        {
            PenLineCap lineCap = Element.StrokeLineCap;
            Paint.Cap aLineCap = Paint.Cap.Butt;

            switch (lineCap)
            {
                case PenLineCap.Flat:
                    aLineCap = Paint.Cap.Butt;
                    break;
                case PenLineCap.Square:
                    aLineCap = Paint.Cap.Square;
                    break;
                case PenLineCap.Round:
                    aLineCap = Paint.Cap.Round;
                    break;
            }

            Control.UpdateStrokeLineCap(aLineCap);
        }

        void UpdateStrokeLineJoin()
        {
            PenLineJoin lineJoin = Element.StrokeLineJoin;
            Paint.Join aLineJoin = Paint.Join.Miter;

            switch (lineJoin)
            {
                case PenLineJoin.Miter:
                    aLineJoin = Paint.Join.Miter;
                    break;
                case PenLineJoin.Bevel:
                    aLineJoin = Paint.Join.Bevel;
                    break;
                case PenLineJoin.Round:
                    aLineJoin = Paint.Join.Round;
                    break;
            }

            Control.UpdateStrokeLineJoin(aLineJoin);
        }
    }

    public class ShapeView : AView
    {
        readonly ShapeDrawable _drawable;
        protected float _density;

        APath _path;
        readonly RectF _pathFillBounds;
        readonly RectF _pathStrokeBounds;

        AColor _stroke;
        AColor _fill;

        float _strokeWidth;
        float[] _strokeDash;

        Stretch _aspect;

        public ShapeView(Context context) : base(context)
        {
            _drawable = new ShapeDrawable(null);

            _density = Resources.DisplayMetrics.Density;

            _pathFillBounds = new RectF();
            _pathStrokeBounds = new RectF();

            _aspect = Stretch.None;
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            if (_path == null)
                return;

            AMatrix transformMatrix = CreateMatrix();

            _path.Transform(transformMatrix);
            transformMatrix.MapRect(_pathFillBounds);
            transformMatrix.MapRect(_pathStrokeBounds);

            if (_fill != null)
            {
                _drawable.Paint.SetStyle(Paint.Style.Fill);
                _drawable.Paint.Color = _fill;
                _drawable.Draw(canvas);
                _drawable.Paint.SetShader(null);
            }

            if (_stroke != null)
            {
                _drawable.Paint.SetStyle(Paint.Style.Stroke);
                _drawable.Paint.Color = _stroke;
                _drawable.Draw(canvas);
                _drawable.Paint.SetShader(null);
            }

            AMatrix inverseTransformMatrix = new AMatrix();
            transformMatrix.Invert(inverseTransformMatrix);
            _path.Transform(inverseTransformMatrix);
            inverseTransformMatrix.MapRect(_pathFillBounds);
            inverseTransformMatrix.MapRect(_pathStrokeBounds);
        }

        public void UpdateShape(APath path)
        {
            _path = path;
            UpdatePathShape();
        }

        public SizeRequest GetDesiredSize()
        {
            if (_path != null)
            {
                return new SizeRequest(new Size(
                    Math.Max(0, _pathStrokeBounds.Right),
                    Math.Max(0, _pathStrokeBounds.Bottom)));
            }

            return new SizeRequest();
        }

        public void UpdateAspect(Stretch aspect)
        {
            _aspect = aspect;
            Invalidate();
        }

        public void UpdateFill(AColor fill)
        {
            _fill = fill;
            Invalidate();
        }

        public void UpdateStroke(AColor stroke)
        {
            _stroke = stroke;
            Invalidate();
        }

        public void UpdateStrokeThickness(float strokeWidth)
        {
            _strokeWidth = _density * strokeWidth;
            _drawable.Paint.StrokeWidth = _strokeWidth;
            UpdatePathStrokeBounds();
        }

        public void UpdateStrokeDashArray(float[] dash)
        {
            _strokeDash = dash;

            if (_strokeDash != null && _strokeDash.Length > 1)
            {
                float[] strokeDash = new float[_strokeDash.Length];

                for (int i = 0; i < _strokeDash.Length; i++)
                    strokeDash[i] = _strokeDash[i] * _strokeWidth;

                _drawable.Paint.SetPathEffect(new DashPathEffect(strokeDash, 0));
            }
            else
                _drawable.Paint.SetPathEffect(null);

            UpdatePathStrokeBounds();
        }

        public void UpdateStrokeLineCap(Paint.Cap strokeCap)
        {
            _drawable.Paint.StrokeCap = strokeCap;
            UpdatePathStrokeBounds();
        }

        public void UpdateStrokeLineJoin(Paint.Join strokeJoin)
        {
            _drawable.Paint.StrokeJoin = strokeJoin;
            Invalidate();
        }

        public void UpdateSize(double width, double height)
        {
            _drawable.SetBounds(0, 0, (int)(width * _density), (int)(height * _density));
            UpdatePathShape();
        }

        protected void UpdatePathShape()
        {
            if (_path != null && !_drawable.Bounds.IsEmpty)
                _drawable.Shape = new PathShape(_path, _drawable.Bounds.Width(), _drawable.Bounds.Height());
            else
                _drawable.Shape = null;

            if (_path != null)
            {
                using (APath fillPath = new APath())
                {
                    _drawable.Paint.StrokeWidth = 0.01f;
                    _drawable.Paint.SetStyle(Paint.Style.Stroke);
                    _drawable.Paint.GetFillPath(_path, fillPath);
                    fillPath.ComputeBounds(_pathFillBounds, false);
                    _drawable.Paint.StrokeWidth = _strokeWidth;
                }
            }
            else
            {
                _pathFillBounds.SetEmpty();
            }

            UpdatePathStrokeBounds();
        }

        AMatrix CreateMatrix()
        {
            AMatrix matrix = new AMatrix();

            RectF drawableBounds = new RectF(_drawable.Bounds);
            float halfStrokeWidth = _drawable.Paint.StrokeWidth / 2;

            drawableBounds.Left += halfStrokeWidth;
            drawableBounds.Top += halfStrokeWidth;
            drawableBounds.Right -= halfStrokeWidth;
            drawableBounds.Bottom -= halfStrokeWidth;

            switch (_aspect)
            {
                case Stretch.None:
                    break;
                case Stretch.Fill:
                    matrix.SetRectToRect(_pathFillBounds, drawableBounds, AMatrix.ScaleToFit.Fill);
                    break;
                case Stretch.Uniform:
                    matrix.SetRectToRect(_pathFillBounds, drawableBounds, AMatrix.ScaleToFit.Center);
                    break;
                case Stretch.UniformToFill:
                    float widthScale = drawableBounds.Width() / _pathFillBounds.Width();
                    float heightScale = drawableBounds.Height() / _pathFillBounds.Height();
                    float maxScale = Math.Max(widthScale, heightScale);
                    matrix.SetScale(maxScale, maxScale);
                    matrix.PostTranslate(
                        drawableBounds.Left - maxScale * _pathFillBounds.Left,
                        drawableBounds.Top - maxScale * _pathFillBounds.Top);
                    break;
            }

            return matrix;
        }

        void UpdatePathStrokeBounds()
        {
            if (_path != null)
            {
                using (APath strokePath = new APath())
                {
                    _drawable.Paint.SetStyle(Paint.Style.Stroke);
                    _drawable.Paint.GetFillPath(_path, strokePath);
                    strokePath.ComputeBounds(_pathStrokeBounds, false);
                }
            }
            else
            {
                _pathStrokeBounds.SetEmpty();
            }

            Invalidate();
        }
    }
}