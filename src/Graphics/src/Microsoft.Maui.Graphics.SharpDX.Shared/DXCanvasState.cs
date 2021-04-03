using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using System;

namespace Microsoft.Maui.Graphics.SharpDX
{
    public class DXCanvasState : CanvasState
    {
        private readonly DXCanvasState _parentState;
        private readonly RenderTarget _renderTarget;
        private float _alpha = 1;
        private float[] _dashes;

        private Brush _fillBrush;
        private bool _fillBrushValid;
        private SolidColorBrush _fontBrush;
        private bool _fontBrushValid;
        private float _fontSize;
        private Vector2 _gradientPoint1;
        private Vector2 _gradientPoint2;
        private GradientStopCollection _gradientStopCollection;
        private RectangleGeometry _layerBounds;
        private RectangleGeometry _layerClipBounds;
        private PathGeometry _layerMask;
        private bool _needsStrokeStyle;
        private float _scale;

        private Color4 _shadowColor;
        private bool _shadowColorValid;
        private Color _sourceFillColor;
        private Paint _sourceFillpaint;

        private Color _sourceFontColor;
        private Color _sourceShadowColor;
        private Color _sourceStrokeColor;
        private SolidColorBrush _strokeBrush;
        private bool _strokeBrushValid;
        private StrokeStyle _strokeStyle;
        private StrokeStyleProperties _strokeStyleProperties;

        private int _layerCount;
        private readonly float _dpi;

        public DXCanvasState(float dpi, RenderTarget renderTarget)
        {
            _dpi = dpi;
            _renderTarget = renderTarget;
            _parentState = null;
            SetToDefaults();
        }

        public DXCanvasState(DXCanvasState prototype) : base(prototype)
        {
            _dpi = prototype.Dpi;
            _parentState = prototype;

            _renderTarget = prototype._renderTarget;

            _strokeBrush = prototype._strokeBrush;
            _fillBrush = prototype._fillBrush;
            _fontBrush = prototype._fontBrush;
            _shadowColor = prototype._shadowColor;

            _sourceStrokeColor = prototype._sourceStrokeColor;
            _sourceFillpaint = prototype._sourceFillpaint;
            _sourceFillColor = prototype._sourceFillColor;
            _sourceFontColor = prototype._sourceFontColor;
            _sourceShadowColor = prototype._sourceShadowColor;

            _strokeBrushValid = prototype._strokeBrushValid;
            _fillBrushValid = prototype._fillBrushValid;
            _fontBrushValid = prototype._fontBrushValid;
            _shadowColorValid = prototype._shadowColorValid;

            _dashes = prototype._dashes;
            _strokeStyle = prototype._strokeStyle;
            _strokeStyleProperties = prototype._strokeStyleProperties;
            _needsStrokeStyle = prototype._needsStrokeStyle;

            IsShadowed = prototype.IsShadowed;
            ShadowOffset = prototype.ShadowOffset;
            ShadowBlur = prototype.ShadowBlur;

            Matrix = new Matrix3x2(prototype.Matrix.ToArray());

            FontName = prototype.FontName;
            FontSize = prototype.FontSize;
            FontWeight = prototype.FontWeight;
            FontStyle = prototype.FontStyle;

            _alpha = prototype._alpha;
            _scale = prototype._scale;

            IsBlurred = prototype.IsBlurred;
            BlurRadius = prototype.BlurRadius;
        }

        public float Dpi => _dpi;

        public float ActualScale => _scale;

        public float ActualShadowBlur => ShadowBlur * Math.Abs(_scale);

        public Vector2 ShadowOffset { get; set; }
        public float ShadowBlur { get; set; }

        public Matrix3x2 Matrix { get; private set; }

        public String FontName { get; set; }
        public FontWeight FontWeight { get; set; }
        public FontStyle FontStyle { get; set; }

        private RenderingParams TextRenderingParams { get; set; }
        private DrawingStateBlock DrawingStateBlock { get; set; }


        public float FontSize
        {
            get => _fontSize;
            set => _fontSize = value;
        }

        public StrokeStyle StrokeStyle
        {
            get
            {
                if (_needsStrokeStyle)
                {
                    if (_strokeStyle == null)
                    {
                        _strokeStyle = _dashes != null
                            ? new StrokeStyle(_renderTarget.Factory, _strokeStyleProperties, _dashes)
                            : new StrokeStyle(_renderTarget.Factory, _strokeStyleProperties);
                    }

                    return _strokeStyle;
                }

                return _strokeStyle;
            }
        }

        public float MiterLimit
        {
            set
            {
                _strokeStyleProperties.MiterLimit = value;
                InvalidateStrokeStyle();
                _needsStrokeStyle = true;
            }
        }

        public Color StrokeColor
        {
            set
            {
                Color vValue = value ?? Colors.Black;

                if (!vValue.Equals(_sourceStrokeColor))
                {
                    _sourceStrokeColor = vValue;
                    _strokeBrushValid = false;
                }
            }
        }

        public LineCap StrokeLineCap
        {
            set
            {
                switch (value)
                {
                    case LineCap.Butt:
                        _strokeStyleProperties.EndCap = CapStyle.Flat;
                        _strokeStyleProperties.StartCap = CapStyle.Flat;
                        break;
                    case LineCap.Round:
                        _strokeStyleProperties.EndCap = CapStyle.Round;
                        _strokeStyleProperties.StartCap = CapStyle.Round;
                        break;
                    default:
                        _strokeStyleProperties.EndCap = CapStyle.Square;
                        _strokeStyleProperties.StartCap = CapStyle.Square;
                        break;
                }

                InvalidateStrokeStyle();
                _needsStrokeStyle = true;
            }
        }

        public LineJoin StrokeLineJoin
        {
            set
            {
                switch (value)
                {
                    case LineJoin.Bevel:
                        _strokeStyleProperties.LineJoin = global::SharpDX.Direct2D1.LineJoin.Bevel;
                        break;
                    case LineJoin.Round:
                        _strokeStyleProperties.LineJoin = global::SharpDX.Direct2D1.LineJoin.Round;
                        break;
                    default:
                        _strokeStyleProperties.LineJoin = global::SharpDX.Direct2D1.LineJoin.Miter;
                        break;
                }

                InvalidateStrokeStyle();
                _needsStrokeStyle = true;
            }
        }

        public Color FillColor
        {
            set
            {
                ReleaseFillBrush();
                _fillBrushValid = false;
                _sourceFillColor = value;
                _sourceFillpaint = null;
            }
        }

        public float BlurRadius { get; private set; }
        public bool IsShadowed { get; set; }
        public bool IsBlurred { get; private set; }

        public Color FontColor
        {
            get => _sourceFontColor ?? Colors.Black;
            set
            {
                Color vValue = value ?? Colors.Black;

                if (!vValue.Equals(_sourceFontColor))
                {
                    _sourceFontColor = vValue;
                    _fontBrushValid = false;
                }
            }
        }

        public float Alpha
        {
            get => _alpha;

            set
            {
                if (_alpha != value)
                {
                    _alpha = value;
                    InvalidateBrushes();
                }
            }
        }

        public SolidColorBrush DxStrokeBrush
        {
            get
            {
                if (_strokeBrush == null ||
                    (!_strokeBrushValid && _parentState != null && _strokeBrush == _parentState._strokeBrush))
                {
                    _strokeBrush = new SolidColorBrush(_renderTarget, _sourceStrokeColor.AsDxColor(_alpha));
                    _strokeBrushValid = true;
                }
                else if (!_strokeBrushValid)
                {
                    _strokeBrush.Color = _sourceStrokeColor.AsDxColor(_alpha);
                    _strokeBrushValid = true;
                }

                return _strokeBrush;
            }
        }

        public Brush DxFillBrush
        {
            get
            {
                if (_fillBrush == null || !_fillBrushValid)
                {
                    if (_sourceFillColor != null)
                    {
                        _fillBrush = new SolidColorBrush(_renderTarget, _sourceFillColor.AsDxColor(_alpha));
                        _fillBrushValid = true;
                    }
                    else if (_sourceFillpaint != null)
                    {
                        if (_sourceFillpaint.PaintType == PaintType.LinearGradient)
                        {
                            var linearGradientProperties = new LinearGradientBrushProperties();
                            var gradientStops = new global::SharpDX.Direct2D1.GradientStop[_sourceFillpaint.Stops.Length];
                            for (int i = 0; i < _sourceFillpaint.Stops.Length; i++)
                            {
                                gradientStops[i] = new global::SharpDX.Direct2D1.GradientStop
                                {
                                    Position = _sourceFillpaint.Stops[i].Offset,
                                    Color = _sourceFillpaint.Stops[i].Color.AsDxColor(_alpha)
                                };
                            }

                            _gradientStopCollection = new GradientStopCollection(_renderTarget, gradientStops, ExtendMode.Clamp);
                            _fillBrush = new LinearGradientBrush(_renderTarget, linearGradientProperties, _gradientStopCollection);
                            ((LinearGradientBrush) _fillBrush).StartPoint = _gradientPoint1;
                            ((LinearGradientBrush) _fillBrush).EndPoint = _gradientPoint2;
                        }
                        else
                        {
                            float radius = Geometry.GetDistance(_gradientPoint1.X, _gradientPoint1.Y, _gradientPoint2.X, _gradientPoint2.Y);

                            var radialGradientBrushProperties = new RadialGradientBrushProperties();
                            var gradientStops = new global::SharpDX.Direct2D1.GradientStop[_sourceFillpaint.Stops.Length];
                            for (int i = 0; i < _sourceFillpaint.Stops.Length; i++)
                            {
                                gradientStops[i] = new global::SharpDX.Direct2D1.GradientStop
                                {
                                    Position = _sourceFillpaint.Stops[i].Offset,
                                    Color = _sourceFillpaint.Stops[i].Color.AsDxColor(_alpha)
                                };
                            }

                            _gradientStopCollection = new GradientStopCollection(_renderTarget, gradientStops, ExtendMode.Clamp);
                            _fillBrush = new RadialGradientBrush(_renderTarget, radialGradientBrushProperties, _gradientStopCollection);
                            ((RadialGradientBrush) _fillBrush).Center = _gradientPoint1;
                            ((RadialGradientBrush) _fillBrush).RadiusX = radius;
                            ((RadialGradientBrush) _fillBrush).RadiusY = radius;
                        }

                        _fillBrushValid = true;
                    }
                    else
                    {
                        _fillBrush = new SolidColorBrush(_renderTarget, global::SharpDX.Color.White);
                        _fillBrushValid = true;
                    }
                }

                return _fillBrush;
            }
        }

        public Color4 ShadowColor
        {
            get
            {
                if (!_shadowColorValid)
                {
                    _shadowColor = _sourceShadowColor?.AsDxColor(_alpha) ?? CanvasDefaults.DefaultShadowColor.AsDxColor(_alpha);
                }

                return _shadowColor;
            }
        }

        public SolidColorBrush FontBrush
        {
            get
            {
                if (_fontBrush == null || (!_fontBrushValid && _parentState != null && _fontBrush == _parentState._fontBrush))
                {
                    _fontBrush = new SolidColorBrush(_renderTarget, _sourceFontColor.AsDxColor(_alpha));
                }
                else if (!_fontBrushValid)
                {
                    _fontBrush.Color = _sourceFontColor.AsDxColor(_alpha);
                }

                return _fontBrush;
            }
        }

        public override void Dispose()
        {
            CloseResources();

            if (TextRenderingParams != null)
            {
                TextRenderingParams.Dispose();
                TextRenderingParams = null;
            }

            if (DrawingStateBlock != null)
            {
                DrawingStateBlock.Dispose();
                DrawingStateBlock = null;
            }

            if (_strokeBrush != null && (_parentState == null || _strokeBrush != _parentState._strokeBrush))
            {
                _strokeBrush.Dispose();
                _strokeBrush = null;
            }

            if (_fontBrush != null && (_parentState == null || _fontBrush != _parentState._fontBrush))
            {
                _fontBrush.Dispose();
                _fontBrush = null;
            }

            if (_strokeStyle != null && (_parentState == null || _strokeStyle != _parentState._strokeStyle))
            {
                _strokeStyle.Dispose();
                _strokeStyle = null;
            }

            if (_fillBrush != null && (_parentState == null || _fillBrush != _parentState._fillBrush))
            {
                _fillBrush.Dispose();
                _fillBrush = null;
            }

            if (_gradientStopCollection != null)
            {
                _gradientStopCollection.Dispose();
                _gradientStopCollection = null;
            }

            base.Dispose();
        }

        public void SaveRenderTargetState()
        {
            /* DrawingStateDescription = new DrawingStateDescription();
            TextRenderingParams = new RenderingParams(DXGraphicsService.FactoryDirectWrite);
            DrawingStateBlock = new DrawingStateBlock(renderTarget.Factory, DrawingStateDescription, TextRenderingParams);
            renderTarget.SaveDrawingState(DrawingStateBlock);*/
        }

        public void CloseResources()
        {
            if (_layerMask != null)
            {
                _renderTarget.PopLayer();

                _layerMask.Dispose();
                _layerMask = null;
            }

            if (_layerBounds != null)
            {
                _layerBounds.Dispose();
                _layerBounds = null;
            }

            if (_layerClipBounds != null)
            {
                _layerClipBounds.Dispose();
                _layerClipBounds = null;
            }
        }

        public void RestoreRenderTargetState()
        {
            /*if (DrawingStateBlock != null)
            {
                renderTarget.RestoreDrawingState(DrawingStateBlock);
                DrawingStateBlock.Dispose();
                DrawingStateBlock = null;

                TextRenderingParams.Dispose();
                TextRenderingParams = null;
            }*/

            _renderTarget.Transform = Matrix;
        }

        public void SetToDefaults()
        {
            _sourceStrokeColor = Colors.Black;
            _strokeBrushValid = false;
            _needsStrokeStyle = false;
            _strokeStyle = null;

            _strokeStyleProperties.DashCap = CapStyle.Flat;
            _strokeStyleProperties.DashOffset = 0;
            _strokeStyleProperties.DashStyle = DashStyle.Solid;
            _strokeStyleProperties.EndCap = CapStyle.Flat;
            _strokeStyleProperties.LineJoin = global::SharpDX.Direct2D1.LineJoin.Miter;
            _strokeStyleProperties.MiterLimit = CanvasDefaults.DefaultMiterLimit;
            _strokeStyleProperties.StartCap = CapStyle.Flat;
            _dashes = null;

            _sourceFillpaint = Colors.White.AsPaint();
            _fillBrushValid = false;

            Matrix = Matrix3x2.Identity;

            IsShadowed = false;
            _sourceShadowColor = CanvasDefaults.DefaultShadowColor;

            FontName = "Arial";
            FontSize = 12;
            FontWeight = FontWeight.Regular;
            FontStyle = FontStyle.Normal;
            _sourceFontColor = Colors.Black;
            _fontBrushValid = false;

            _alpha = 1;
            _scale = 1;

            IsBlurred = false;
            BlurRadius = 0;
        }

        public void SetStrokeDashPattern(float[] pattern, float strokeSize)
        {
            if (pattern == null || pattern.Length == 0)
            {
                if (_needsStrokeStyle == false) return;
                _strokeStyleProperties.DashStyle = DashStyle.Solid;
                _dashes = null;
            }
            else
            {
                _strokeStyleProperties.DashStyle = DashStyle.Custom;
                _dashes = pattern;
            }

            InvalidateStrokeStyle();
            _needsStrokeStyle = true;
        }

        private void ReleaseFillBrush()
        {
            if (_fillBrush != null)
            {
                if (_parentState == null || _fillBrush != _parentState._fillBrush)
                {
                    _fillBrush.Dispose();
                    if (_gradientStopCollection != null)
                    {
                        _gradientStopCollection.Dispose();
                        _gradientStopCollection = null;
                    }
                }

                _fillBrush = null;
            }
        }

        public void SetLinearGradient(Paint aPaint, Vector2 aPoint1, Vector2 aPoint2)
        {
            ReleaseFillBrush();
            _fillBrushValid = false;
            _sourceFillColor = null;
            _sourceFillpaint = aPaint;
            _gradientPoint1 = aPoint1;
            _gradientPoint2 = aPoint2;
        }

        public void SetRadialGradient(Paint aPaint, Vector2 aPoint1, Vector2 aPoint2)
        {
            ReleaseFillBrush();
            _fillBrushValid = false;
            _sourceFillColor = null;
            _sourceFillpaint = aPaint;
            _gradientPoint1 = aPoint1;
            _gradientPoint2 = aPoint2;
        }

        private void InvalidateStrokeStyle()
        {
            if (_strokeStyle != null)
            {
                if (_parentState == null || _strokeStyle != _parentState._strokeStyle)
                {
                    _strokeStyle.Dispose();
                }

                _strokeStyle = null;
            }
        }

        public void SetShadow(SizeF aOffset, float aBlur, Color aColor)
        {
            if (aOffset != null)
            {
                IsShadowed = true;
                ShadowOffset = new Vector2(aOffset.Width, aOffset.Height);
                ShadowBlur = aBlur;
                _sourceShadowColor = aColor;
                _shadowColorValid = false;
            }
            else
            {
                IsShadowed = false;
            }
        }

        public void SetBlur(float aRadius)
        {
            if (aRadius > 0)
            {
                IsBlurred = true;
                BlurRadius = aRadius;
            }
            else
            {
                IsBlurred = false;
                BlurRadius = 0;
            }
        }

        public Matrix3x2 DxTranslate(float tx, float ty)
        {
            //Matrix = Matrix * Matrix3x2.Translation(tx, ty);
            Matrix = Matrix.Translate(tx, ty);
            return Matrix;
        }

        public Matrix3x2 DxConcatenateTransform(AffineTransform transform)
        {
            var values = new float[6];
            transform.GetMatrix(values);
            Matrix = Matrix3x2.Multiply(Matrix, new Matrix3x2(values));
            return Matrix;
        }

        public Matrix3x2 DxScale(float tx, float ty)
        {
            _scale *= tx;
            Matrix = Matrix.Scale(tx, ty);
            return Matrix;
        }

        public Matrix3x2 DxRotate(float aAngle)
        {
            float radians = Geometry.DegreesToRadians(aAngle);
            Matrix = Matrix.Rotate(radians);
            return Matrix;
        }

        public Matrix3x2 DxRotate(float aAngle, float x, float y)
        {
            float radians = Geometry.DegreesToRadians(aAngle);
            Matrix = Matrix.Translate(x, y);
            Matrix = Matrix.Rotate(radians);
            Matrix = Matrix.Translate(-x, -y);
            return Matrix;
        }

        private void InvalidateBrushes()
        {
            _strokeBrushValid = false;
            _fillBrushValid = false;
            _shadowColorValid = false;
            _fontBrushValid = false;
        }

        public void ClipPath(PathF path, WindingMode windingMode)
        {
            if (_layerMask != null)
            {
                throw new Exception("Only one clip operation currently supported.");
            }

            _layerMask = new PathGeometry(_renderTarget.Factory);
            var sink = _layerMask.Open();
            sink.SetFillMode(windingMode == WindingMode.NonZero ? FillMode.Winding : FillMode.Alternate);

            var layerRect = new global::SharpDX.RectangleF(0, 0, _renderTarget.Size.Width, _renderTarget.Size.Height);
            _layerBounds = new RectangleGeometry(_renderTarget.Factory, layerRect);

            var clipGeometry = path.AsDxPath(_renderTarget.Factory);

            _layerBounds.Combine(clipGeometry, CombineMode.Intersect, sink);
            sink.Close();

            var layerParameters = new LayerParameters1
            {
                ContentBounds = layerRect,
                MaskTransform = Matrix3x2.Identity,
                MaskAntialiasMode = AntialiasMode.PerPrimitive,
                Opacity = 1,
                GeometricMask = _layerMask
            };

            ((DeviceContext) _renderTarget).PushLayer(layerParameters, null);
            _layerCount++;
        }

        public void ClipRectangle(float x, float y, float width, float height)
        {
            var path = new PathF();
            path.AppendRectangle(x, y, width, height);
            ClipPath(path, WindingMode.NonZero);
        }

        public void SubtractFromClip(float x, float y, float width, float height)
        {
            if (_layerMask != null)
            {
                throw new Exception("Only one subtraction currently supported.");
            }

            _layerMask = new PathGeometry(_renderTarget.Factory);
            var maskSink = _layerMask.Open();

            var layerRect = new global::SharpDX.RectangleF(0, 0, _renderTarget.Size.Width, _renderTarget.Size.Height);
            _layerBounds = new RectangleGeometry(_renderTarget.Factory, layerRect);

            var boundsToSubtract = new global::SharpDX.RectangleF(x, y, width, height);
            _layerClipBounds = new RectangleGeometry(_renderTarget.Factory, boundsToSubtract);

            _layerBounds.Combine(_layerClipBounds, CombineMode.Exclude, maskSink);
            maskSink.Close();

            var layerParameters = new LayerParameters1
            {
                ContentBounds = layerRect,
                MaskTransform = Matrix3x2.Identity,
                MaskAntialiasMode = AntialiasMode.PerPrimitive,
                Opacity = 1,
                GeometricMask = _layerMask
            };

            ((DeviceContext) _renderTarget).PushLayer(layerParameters, null);
            _layerCount++;
        }

        public void SetBitmapBrush(Brush bitmapBrush)
        {
            _fillBrush = bitmapBrush;
            _fillBrushValid = true;
        }
    }
}
