using System;
using System.ComponentModel;
using CoreAnimation;
using CoreGraphics;
using Xamarin.Forms.Shapes;
using Shape = Xamarin.Forms.Shapes.Shape;

#if __MOBILE__
using UIKit;

namespace Xamarin.Forms.Platform.iOS
#else
using AppKit;

namespace Xamarin.Forms.Platform.MacOS
#endif
{
    public class ShapeRenderer<TShape, TNativeShape> : ViewRenderer<TShape, TNativeShape>
        where TShape : Shape
        where TNativeShape : ShapeView
    {
        double _height;
        double _width;

        protected override void OnElementChanged(ElementChangedEventArgs<TShape> args)
        {
            base.OnElementChanged(args);

            if (args.NewElement != null)
            {
                UpdateAspect();
                UpdateFill();
                UpdateStroke();
                UpdateStrokeThickness();
                UpdateStrokeDashArray();
                UpdateStrokeDashOffset();
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
            else if (args.PropertyName == Shape.StrokeDashOffsetProperty.PropertyName)
                UpdateStrokeDashOffset();
            else if (args.PropertyName == Shape.StrokeLineCapProperty.PropertyName)
                UpdateStrokeLineCap();
            else if (args.PropertyName == Shape.StrokeLineJoinProperty.PropertyName)
                UpdateStrokeLineJoin();
        }

        public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            if (Control != null)
            {
                return Control.ShapeLayer.GetDesiredSize();
            }

            return base.GetDesiredSize(widthConstraint, heightConstraint);
        }

        void UpdateAspect()
        {
            Control.ShapeLayer.UpdateAspect(Element.Aspect);
        }

        void UpdateSize()
        {
            Control.ShapeLayer.UpdateSize(new CGSize(new nfloat(_width), new nfloat(_height)));
        }

        void UpdateFill()
        {
            Control.ShapeLayer.UpdateFill(Element.Fill.ToCGColor());
        }

        void UpdateStroke()
        {
            Control.ShapeLayer.UpdateStroke(Element.Stroke.ToCGColor());
        }

        void UpdateStrokeThickness()
        {
            Control.ShapeLayer.UpdateStrokeThickness(Element.StrokeThickness);
        }

        void UpdateStrokeDashArray()
        {
            Control.ShapeLayer.UpdateStrokeDash(Element.StrokeDashArray.ToArray());
        }

        void UpdateStrokeDashOffset()
        {
            Control.ShapeLayer.UpdateStrokeDashOffset((nfloat)Element.StrokeDashOffset);
        }

		void UpdateStrokeLineCap()
		{
            PenLineCap lineCap = Element.StrokeLineCap;
            CGLineCap iLineCap = CGLineCap.Butt;

            switch (lineCap)
            {
                case PenLineCap.Flat:
                    iLineCap = CGLineCap.Butt;
                    break;
                case PenLineCap.Square:
                    iLineCap = CGLineCap.Square;
                    break;
                case PenLineCap.Round:
                    iLineCap = CGLineCap.Round;
                    break;
            }

            Control.ShapeLayer.UpdateStrokeLineCap(iLineCap);
        }

        void UpdateStrokeLineJoin()
        {
            PenLineJoin lineJoin = Element.StrokeLineJoin;
            CGLineJoin iLineJoin = CGLineJoin.Miter;

            switch (lineJoin)
            {
                case PenLineJoin.Miter:
                    iLineJoin = CGLineJoin.Miter;
                    break;
                case PenLineJoin.Bevel:
                    iLineJoin = CGLineJoin.Bevel;
                    break;
                case PenLineJoin.Round:
                    iLineJoin = CGLineJoin.Round;
                    break;
            }

            Control.ShapeLayer.UpdateStrokeLineJoin(iLineJoin);
        }
    }

    public class ShapeView
#if __MOBILE__
	: UIView
#else
    : NSView
#endif
    {
        public ShapeView()
        {
#if __MOBILE__
            BackgroundColor = UIColor.Clear;
#else
            WantsLayer = true;
#endif
            ShapeLayer = new ShapeLayer();
            Layer.AddSublayer(ShapeLayer);
            Layer.MasksToBounds = false;
        }

        public ShapeLayer ShapeLayer
        {
            private set;
            get;
        }

#if !__MOBILE__
        public override bool IsFlipped => true;
#endif
    }

    public class ShapeLayer : CALayer
    {
        const float StrokeMiterLimit = 10f;

        CGPath _path;
        CGRect _pathFillBounds;
        CGRect _pathStrokeBounds;

        CGPath _renderPath;

        bool _fillMode;

        CGColor _stroke;
        CGColor _fill;

        nfloat _strokeWidth;
        nfloat[] _strokeDash;
        nfloat _dashOffset;

        Stretch _stretch;

		CGLineCap _strokeLineCap;
        CGLineJoin _strokeLineJoin;

        public ShapeLayer()
        {
            _fillMode = false;
            _stretch = Stretch.None;
            _strokeLineCap = CGLineCap.Butt;
            _strokeLineJoin = CGLineJoin.Miter;
        }

        public override void DrawInContext(CGContext ctx)
        {
            base.DrawInContext(ctx);
            RenderShape(ctx);
        }

        public void UpdateShape(CGPath path)
        {
            _path = path;

            if (_path != null)
                _pathFillBounds = _path.PathBoundingBox;
            else
                _pathFillBounds = new CGRect();

            UpdatePathStrokeBounds();
        }

        public void UpdateFillMode(bool fillMode)
        {
            _fillMode = fillMode;
            SetNeedsDisplay();
        }

        public SizeRequest GetDesiredSize()
        {
            return new SizeRequest(new Size(
                Math.Max(0, _pathStrokeBounds.Right),
                Math.Max(0, _pathStrokeBounds.Bottom)));
        }

        public void UpdateSize(CGSize size)
        {
            Bounds = new CGRect(new CGPoint(), size);
            BuildRenderPath();
        }

        public void UpdateAspect(Stretch stretch)
        {
            _stretch = stretch;
            BuildRenderPath();
        }

        public void UpdateFill(CGColor fill)
        {
            _fill = fill;
            SetNeedsDisplay();
        }

        public void UpdateStroke(CGColor stroke)
        {
            _stroke = stroke;
            SetNeedsDisplay();
        }

        public void UpdateStrokeThickness(double strokeWidth)
        {
            _strokeWidth = new nfloat(strokeWidth);
            BuildRenderPath();
        }

        public void UpdateStrokeDash(nfloat[] dash)
        {
            _strokeDash = dash;
            SetNeedsDisplay();
        }

        public void UpdateStrokeDashOffset(nfloat dashOffset)
        {
            _dashOffset = dashOffset;
            SetNeedsDisplay();
        }

        public void UpdateStrokeLineCap(CGLineCap strokeLineCap)
        {
            _strokeLineCap = strokeLineCap;
            UpdatePathStrokeBounds();
            SetNeedsDisplay();
        }

        public void UpdateStrokeLineJoin(CGLineJoin strokeLineJoin)
        {
            _strokeLineJoin = strokeLineJoin;
            UpdatePathStrokeBounds();
            SetNeedsDisplay();
        }

        void BuildRenderPath()
        {
            if (_path == null)
            {
                _renderPath = null;
                return;
            }

            CATransaction.Begin();
            CATransaction.DisableActions = true;

            if (_stretch != Stretch.None)
            {
                CGRect viewBounds = Bounds;
                viewBounds.X += _strokeWidth / 2;
                viewBounds.Y += _strokeWidth / 2;
                viewBounds.Width -= _strokeWidth;
                viewBounds.Height -= _strokeWidth;

                nfloat widthScale = viewBounds.Width / _pathFillBounds.Width;
                nfloat heightScale = viewBounds.Height / _pathFillBounds.Height;
                var stretchTransform = CGAffineTransform.MakeIdentity();

                switch (_stretch)
                {
                    case Stretch.None:
                        break;

                    case Stretch.Fill:
                        stretchTransform.Scale(widthScale, heightScale);

                        stretchTransform.Translate(
                            viewBounds.Left - widthScale * _pathFillBounds.Left,
                            viewBounds.Top - heightScale * _pathFillBounds.Top);
                        break;

                    case Stretch.Uniform:
                        nfloat minScale = NMath.Min(widthScale, heightScale);

                        stretchTransform.Scale(minScale, minScale);

                        stretchTransform.Translate(
                            viewBounds.Left - minScale * _pathFillBounds.Left +
                            (viewBounds.Width - minScale * _pathFillBounds.Width) / 2,
                            viewBounds.Top - minScale * _pathFillBounds.Top +
                            (viewBounds.Height - minScale * _pathFillBounds.Height) / 2);
                        break;

                    case Stretch.UniformToFill:
                        nfloat maxScale = NMath.Max(widthScale, heightScale);

                        stretchTransform.Scale(maxScale, maxScale);

                        stretchTransform.Translate(
                            viewBounds.Left - maxScale * _pathFillBounds.Left,
                            viewBounds.Top - maxScale * _pathFillBounds.Top);
                        break;
                }

                Frame = Bounds;
                _renderPath = _path.CopyByTransformingPath(stretchTransform);
            }
            else
            {
                nfloat adjustX = NMath.Min(0, _pathStrokeBounds.X);
                nfloat adjustY = NMath.Min(0, _pathStrokeBounds.Y);

                if (adjustX < 0 || adjustY < 0)
                {
                    nfloat width = Bounds.Width;
                    nfloat height = Bounds.Height;

                    if (_pathStrokeBounds.Width > Bounds.Width)
                        width = Bounds.Width - adjustX;
                    if (_pathStrokeBounds.Height > Bounds.Height)
                        height = Bounds.Height - adjustY;

                    Frame = new CGRect(adjustX, adjustY, width, height);
                    var transform = new CGAffineTransform(Bounds.Width / width, 0, 0, Bounds.Height / height, -adjustX, -adjustY);
                    _renderPath = _path.CopyByTransformingPath(transform);
                }
                else
                {
                    Frame = Bounds;
                    _renderPath = _path.CopyByTransformingPath(CGAffineTransform.MakeIdentity());
                }
            }

            CATransaction.Commit();

            SetNeedsDisplay();
        }

        void RenderShape(CGContext graphics)
        {
            if (_path == null)
                return;

            if (_stroke == null && _fill == null)
                return;

            CATransaction.Begin();
            CATransaction.DisableActions = true;

            var lengths = new nfloat[0];

            if (_strokeDash.Length > 0)
                lengths = new nfloat[_strokeDash.Length];

            for (int i = 0; i < _strokeDash.Length; i++)
                lengths[i] = new nfloat(_dashOffset * _strokeDash[i]);

            graphics.SetLineWidth(_strokeWidth);
            graphics.SetLineDash(_dashOffset * _strokeWidth, lengths);
            graphics.SetLineCap(_strokeLineCap);
            graphics.SetLineJoin(_strokeLineJoin);
            graphics.SetMiterLimit(StrokeMiterLimit * _strokeWidth / 4);

            graphics.AddPath(_renderPath);
            graphics.SetStrokeColor(_stroke);
            graphics.SetFillColor(_fill);
            graphics.DrawPath(_fillMode ? CGPathDrawingMode.FillStroke : CGPathDrawingMode.EOFillStroke);

            CATransaction.Commit();
        }

        void UpdatePathStrokeBounds()
        {
            if (_path != null)
                _pathStrokeBounds = _path.CopyByStrokingPath(_strokeWidth, _strokeLineCap, _strokeLineJoin, StrokeMiterLimit).PathBoundingBox;
            else
                _pathStrokeBounds = new CGRect();

            BuildRenderPath();
        }
    }
}