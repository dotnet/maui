using System.Numerics;

using Windows.UI.Text;
using Windows.UI.Xaml.Media;

namespace Microsoft.Maui.Graphics.Xaml
{
	public class XamlCanvasState : CanvasState
	{
		private Color _strokeColor = Colors.Black;
		private Color _fillColor = Colors.White;
		private Paint _fillPaint;
		private RectF _fillRectangle;
		private Color _fontColor = Colors.Black;
		private float _alpha = 1;
		private DoubleCollection _dashArray;
		private SizeF _shadowOffset;
		private float _shadowBlur;
		private Color _shadowColor;
		private float _miterLimit = CanvasDefaults.DefaultMiterLimit;
		private LineCap _strokeLineCap = LineCap.Butt;
		private LineJoin _strokeLineJoin = LineJoin.Miter;
		private TransformGroup _transformGroup;
		private bool _transformUsed;
		private double _fontSize;
		private IFont _font;

		public XamlCanvasState()
		{
		}

		public XamlCanvasState(XamlCanvasState prototype) : base(prototype)
		{
			_strokeColor = prototype._strokeColor;
			_fillColor = prototype._fillColor;
			_fillPaint = prototype._fillPaint;
			_fillRectangle = prototype._fillRectangle;
			_fontColor = prototype._fontColor;
			_dashArray = prototype._dashArray;
			_strokeLineCap = prototype._strokeLineCap;
			_strokeLineJoin = prototype._strokeLineJoin;
			_alpha = prototype._alpha;
			_shadowOffset = prototype._shadowOffset;
			_shadowBlur = prototype._shadowBlur;
			_shadowColor = prototype._shadowColor;
			_miterLimit = prototype._miterLimit;
			_transformGroup = CreateCopy(prototype._transformGroup);
			_transformUsed = false;
			_fontSize = prototype._fontSize;
			_font = prototype._font;
		}

		public Brush XamlStrokeBrush => new SolidColorBrush(_strokeColor.AsColor());

		public Brush XamlFillBrush
		{
			get
			{
				if (_fillColor != null)
					return new SolidColorBrush(_fillColor.AsColor());

				if (_fillPaint != null)
				{
					if (_fillPaint is SolidPaint solidPaint)
						return new SolidColorBrush(solidPaint.Color.AsColor());

					if (_fillPaint is LinearGradientPaint linearGradientPaint)
					{
						float x1 = (float)(linearGradientPaint.StartPoint.X * _fillRectangle.Width);
						float y1 = (float)(linearGradientPaint.StartPoint.Y * _fillRectangle.Height);

						float x2 = (float)(linearGradientPaint.EndPoint.X * _fillRectangle.Width);
						float y2 = (float)(linearGradientPaint.EndPoint.Y * _fillRectangle.Height);

						var brush = new LinearGradientBrush
						{
							MappingMode = BrushMappingMode.Absolute,
							StartPoint = new Windows.Foundation.Point(x1, y1),
							EndPoint = new Windows.Foundation.Point(x2, y2)
						};

						foreach (var stop in linearGradientPaint.GradientStops)
							brush.GradientStops.Add(new Windows.UI.Xaml.Media.GradientStop() { Color = stop.Color.AsColor(), Offset = stop.Offset });

						return brush;
					}

					/*if (_fillPaint is RadialGradientPaint radialGradientPaint)
					{
						float centerX = (float)(radialGradientPaint.Center.X * _fillRectangle.Width);
 						float centerY = (float)(radialGradientPaint.Center.Y * _fillRectangle.Height);
 						float radius = (float)radialGradientPaint.Radius * Math.Max(_fillRectangle.Height, _fillRectangle.Width);

						if (radius == 0)
							radius = Geometry.GetDistance(_fillRectangle.Left, _fillRectangle.Top, _fillRectangle.Right, _fillRectangle.Bottom);

						var brush = new RadialGradientBrush();
						brush.MappingMode = BrushMappingMode.Absolute;
						brush.GradientOrigin = brush.Center = new Point(centerX, centerY);

						brush.RadiusX = radius;
						brush.RadiusY = radius;

						foreach (var stop in fillPaint.Stops)
							brush.GradientStops.Add(new GradientStop() { Color = stop.Color.AsColor(), Offset = stop.Offset });

						return brush;
					}*/

					if (_fillPaint is GradientPaint gradientPaint)
						return new SolidColorBrush(gradientPaint.BlendStartAndEndColors().AsColor());
				}

				return new SolidColorBrush(Windows.UI.Colors.White);
			}
		}

		public Brush XamlFontBrush => new SolidColorBrush(_fontColor.AsColor());

		public float Alpha
		{
			get => _alpha;
			set => _alpha = value;
		}

		public float MiterLimit
		{
			get => _miterLimit;
			set => _miterLimit = value;
		}

		public LineCap StrokeLineCap
		{
			get => _strokeLineCap;
			set => _strokeLineCap = value;
		}

		public double FontSize
		{
			get => _fontSize;
			set => _fontSize = value;
		}

		public LineJoin StrokeLineJoin
		{
			get => _strokeLineJoin;
			set => _strokeLineJoin = value;
		}

		public Color StrokeColor
		{
			set => _strokeColor = value ?? Colors.Black;
		}

		public Color FillColor
		{
			set
			{
				_fillColor = value;
				_fillPaint = null;
			}
		}

		public Color FontColor
		{
			set => _fontColor = value;
		}


		public DoubleCollection XamlDashArray
		{
			get
			{
				if (StrokeDashPattern == null || StrokeDashPattern.Length == 0) return null;
				if (_dashArray == null)
				{
					_dashArray = new DoubleCollection();
					foreach (var value in StrokeDashPattern)
					{
						_dashArray.Add(value);
					}
				}

				return _dashArray;
			}

			set => _dashArray = value;
		}

		public PenLineJoin XamlLineJoin
		{
			get
			{
				switch (_strokeLineJoin)
				{
					case LineJoin.Miter:
						return PenLineJoin.Miter;
					case LineJoin.Bevel:
						return PenLineJoin.Bevel;
					case LineJoin.Round:
						return PenLineJoin.Round;
				}

				return PenLineJoin.Miter;
			}
		}

		public PenLineCap XamlLineCap
		{
			get
			{
				switch (_strokeLineCap)
				{
					case LineCap.Butt:
						return PenLineCap.Flat;
					case LineCap.Square:
						return PenLineCap.Square;
					case LineCap.Round:
						return PenLineCap.Round;
				}

				return PenLineCap.Flat;
			}
		}

		public IFont Font
		{
			set => _font = value;
		}

		public global::Windows.UI.Xaml.Media.FontFamily FontFamily
			=> string.IsNullOrEmpty(_font?.Name) ? FontFamily.XamlAutoFontFamily : new FontFamily(_font?.Name);

		public FontWeight FontWeight
			=> new FontWeight { Weight = (ushort)(_font?.Weight ?? Graphics.Font.Default.Weight) };

		public FontStyle FontStyle
			=> (_font?.StyleType ?? Graphics.Font.Default.StyleType) switch
			{
				FontStyleType.Normal => FontStyle.Normal,
				FontStyleType.Italic => FontStyle.Italic,
				FontStyleType.Oblique => FontStyle.Oblique,
				_ => FontStyle.Normal
			};

		public void SetShadow(SizeF offset, float blur, Color color)
		{
			_shadowOffset = offset;
			_shadowBlur = blur;
			_shadowColor = color;
		}

		public void ResetXamlTransform()
		{
			_transformUsed = false;
			_transformGroup = null;
		}


		public Transform XamlTransform
		{
			get
			{
				if (_transformGroup != null)
				{
					_transformUsed = true;
					return _transformGroup;
				}
				return new MatrixTransform();
			}
		}

		public void XamlTranslate(float tx, float ty)
		{
			if (tx > 0 || tx < 0 || ty > 0 || ty < 0)
			{
				InitGroup();
				var transform = new TranslateTransform() {X = tx, Y = ty};
				_transformGroup.Children.Add(transform);
			}
		}

		private void InitGroup()
		{
			if (_transformGroup == null)
			{
				_transformGroup = new TransformGroup();
				_transformUsed = false;
			}
			else if (_transformUsed)
			{
				_transformGroup = CreateCopy(_transformGroup);
				_transformUsed = false;
			}
		}

		private TransformGroup CreateCopy(TransformGroup prototype)
		{
			if (prototype != null)
			{
				var newGroup = new TransformGroup();
				foreach (var child in prototype.Children)
				{
					if (child is TranslateTransform translateTransform)
					{
						newGroup.Children.Add(new TranslateTransform()
						{
							X = translateTransform.X,
							Y = translateTransform.Y
						});
					}
					else
					{
						if (child is ScaleTransform scaleTransform)
						{
							newGroup.Children.Add(new ScaleTransform()
							{
								ScaleX = scaleTransform.ScaleX,
								ScaleY = scaleTransform.ScaleY
							});
						}
						else
						{
							if (child is RotateTransform rotateTransform)
							{
								newGroup.Children.Add(new RotateTransform()
								{
									CenterX = rotateTransform.CenterX,
									CenterY = rotateTransform.CenterY,
									Angle = rotateTransform.Angle,
								});
							}
							else
							{
								if (child is MatrixTransform matrixTransform)
								{
									var m = matrixTransform.Matrix;
									newGroup.Children.Add(new MatrixTransform()
									{
										Matrix = new Matrix(m.M11, m.M12, m.M21, m.M22, m.OffsetX, m.OffsetY)
									});
								}
							}
						}
					}
				}

				return newGroup;
			}

			return null;
		}

		public void XamlScale(float sx, float sy)
		{
			InitGroup();
			var transform = new ScaleTransform() {ScaleX = sx, ScaleY = sy};
			_transformGroup.Children.Add(transform);
		}

		public void XamlRotate(float degrees, float radians)
		{
			InitGroup();
			var transform = new RotateTransform() { Angle = -degrees, CenterX = 0, CenterY = 0};
			_transformGroup.Children.Add(transform);
		}

		public void XamlRotate(float degrees, float radians, float cx, float cy)
		{
			InitGroup();
			var transform = new RotateTransform() { Angle = -degrees, CenterX = cx, CenterY = cy };
			_transformGroup.Children.Add(transform);
		}

		public void XamlConcatenateTransform(Matrix3x2 transform)
		{
			InitGroup();
			var nativeTransform = transform.AsTransform();
			_transformGroup.Children.Add(nativeTransform);
		}

		public Transform GetXamlTransform(double x, double y)
		{
			if (_transformGroup == null)
			{
				return new MatrixTransform();
			}

			var group = CreateCopy(_transformGroup);
			if (x > 0 || x < 0 || y > 0 || y < 0)
			{
				foreach (var transform in group.Children)
				{
					if (transform is RotateTransform rotation)
					{
						rotation.CenterX -= x;
						rotation.CenterY -= y;
					}
				}
			}

			return group;
		}

		internal void SetFillPaint(Paint paint, RectF rectangle)
		{
			_fillColor = null;
			_fillPaint = paint;
			_fillRectangle = rectangle;
		}
	}
}
