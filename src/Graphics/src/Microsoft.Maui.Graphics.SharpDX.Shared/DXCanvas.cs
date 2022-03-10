using System.Collections.Generic;
using System.Globalization;
using Microsoft.Maui.Graphics.Text;
#if WINDOWS_UWP
using Windows.Graphics.Display;
#endif
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct2D1.Effects;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode;
using System;

using NumericsMatrix3x2 = System.Numerics.Matrix3x2;

namespace Microsoft.Maui.Graphics.SharpDX
{
	public class DXCanvas : AbstractCanvas<DXCanvasState>, IBlurrableCanvas
	{
		private const float Dip = 96f / 72f;

		//public static Factory SharedFactory = new Factory(FactoryType.MultiThreaded);

		//public static readonly ThreadLocal<Factory> CurrentFactory = new ThreadLocal<Factory>();
		//public static readonly ThreadLocal<RenderTarget> CurrentTarget = new ThreadLocal<RenderTarget>();
		//public static ImagingFactory2 FactoryImaging = new ImagingFactory2();

		public static global::SharpDX.DirectWrite.Factory FactoryDirectWrite = new global::SharpDX.DirectWrite.Factory(global::SharpDX.DirectWrite.FactoryType.Shared);




		private static readonly global::SharpDX.RectangleF InfiniteRect = new global::SharpDX.RectangleF(
			float.NegativeInfinity,
			float.NegativeInfinity,
			float.PositiveInfinity,
			float.PositiveInfinity);

		private GaussianBlur _blurEffect;
		private Bitmap1 _effectBitmap;
		private DeviceContext _effectContext;
		private Bitmap1 _patternBitmap;
		private DeviceContext _patternContext;
		private DeviceContext _vectorPatternContext;
		private Ellipse _ellipse;

		private Vector2 _point1;
		private Vector2 _point2;
		private float _radius;
		private RectF _fillRectangle;
		private Size2F _size;
		private global::SharpDX.RectangleF _rect;
		private RenderTarget _renderTarget;
		private RoundedRectangle _roundedRect;
		private Shadow _shadowEffect;

		private global::SharpDX.RectangleF _renderBounds = InfiniteRect;

		private bool _bitmapPatternFills;

		public DXCanvas() : base(CreateNewState, CreateStateCopy)
		{
			Dpi = 96;
		}

		public DXCanvas(RenderTarget renderTarget) : base(CreateNewState, CreateStateCopy)
		{
			RenderTarget = renderTarget;
			Dpi = renderTarget.DotsPerInch.Width;
		}

		private static DXCanvasState CreateNewState(object context)
		{
			var canvas = (DXCanvas) context;
			return new DXCanvasState(canvas.Dpi, canvas.RenderTarget);
		}

		private static DXCanvasState CreateStateCopy(DXCanvasState prototype)
		{
			return new DXCanvasState(prototype);
		}

		private float _dpi = 96;

		public float Dpi
		{
			get => _dpi;
			set => _dpi = value;
		}

		public override float DisplayScale
		{
			get { return _dpi / 96; }
			set { base.DisplayScale = value; }
		}

		public bool BitmapPatternFills
		{
			get => _bitmapPatternFills;
			set => _bitmapPatternFills = value;
		}

		public RenderTarget RenderTarget
		{
			get { return _renderTarget; }
			set
			{
				if (_renderTarget != value)
				{
					if (_renderTarget != null)
					{
						DisposeEffectContext();
					}

					_renderTarget = value;
					ResetState();

					if (_renderTarget != null)
					{
						_renderTarget.TextAntialiasMode = TextAntialiasMode.Default;
						_renderTarget.AntialiasMode = AntialiasMode.PerPrimitive;
						_renderBounds = InfiniteRect;
#if WINDOWS_UWP
						var displayInformation = DisplayInformation.GetForCurrentView();
						Dpi = displayInformation.LogicalDpi;
#endif
					}
				}
				else
				{
					_renderTarget = value;
					ResetState();
				}
			}
		}

		public void SetRenderSize(int width, int height)
		{
			_renderBounds = new global::SharpDX.RectangleF(0, 0, width, height);
		}

		public override float MiterLimit
		{
			set => CurrentState.MiterLimit = value;
		}

		public override Color StrokeColor
		{
			set => CurrentState.StrokeColor = value;
		}

		public override LineCap StrokeLineCap
		{
			set => CurrentState.StrokeLineCap = value;
		}

		public override LineJoin StrokeLineJoin
		{
			set => CurrentState.StrokeLineJoin = value;
		}

		public override Color FillColor
		{
			set => CurrentState.FillColor = value;
		}

		public override Color FontColor
		{
			set => CurrentState.FontColor = value;
		}

		public override IFont Font
		{
			set
			{
				if (value == null)
					value = Graphics.Font.Default;

				CurrentState.Font = value;
			}
		}

		public override float FontSize
		{
			set => CurrentState.FontSize = value;
		}

		public override float Alpha
		{
			set => CurrentState.Alpha = value;
		}

		public override bool Antialias
		{
			set
			{
				if (value)
				{
					_renderTarget.AntialiasMode = AntialiasMode.PerPrimitive;
					_renderTarget.TextAntialiasMode = TextAntialiasMode.Default;
				}
				else
				{
					_renderTarget.AntialiasMode = AntialiasMode.Aliased;
					_renderTarget.TextAntialiasMode = TextAntialiasMode.Aliased;
				}
			}
		}

		public override BlendMode BlendMode
		{
			set
			{
				/* todo: implement me */
				// This will require the use of the bitmap compositing like with shadows.
			}
		}

		public void SetBlur(float blurRadius)
		{
			CurrentState.SetBlur(blurRadius);
		}

		public override void Dispose()
		{
			RenderTarget = null;
			DisposeEffectContext();
			base.Dispose();
		}

		private void DisposeEffectContext()
		{
				_effectContext = null;
				_effectBitmap = null;
				_shadowEffect = null;
				_blurEffect = null;
				_patternContext = null;
				_patternBitmap = null;
				_vectorPatternContext = null;
		}

		protected override void PlatformDrawLine(float x1, float y1, float x2, float y2)
		{
			_point1.X = x1;
			_point1.Y = y1;
			_point2.X = x2;
			_point2.Y = y2;

			Draw(ctx => ctx.DrawLine(_point1, _point2, CurrentState.DxStrokeBrush, CurrentState.StrokeSize, CurrentState.StrokeStyle));
		}

		protected override void PlatformDrawArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed)
		{
			while (startAngle < 0)
			{
				startAngle += 360;
			}

			while (endAngle < 0)
			{
				endAngle += 360;
			}

			var rotation = Geometry.GetSweep(startAngle, endAngle, clockwise);
			var absRotation = Math.Abs(rotation);

			float strokeWidth = CurrentState.StrokeSize;


			SetRect(x, y, width, height);

			_size.Width = _rect.Width / 2;
			_size.Height = _rect.Height / 2;

			var startPoint = Geometry.EllipseAngleToPoint(_rect.X, _rect.Y, _rect.Width, _rect.Height, -startAngle);
			var endPoint = Geometry.EllipseAngleToPoint(_rect.X, _rect.Y, _rect.Width, _rect.Height, -endAngle);

			_point1.X = startPoint.X;
			_point1.Y = startPoint.Y;

			_point2.X = endPoint.X;
			_point2.Y = endPoint.Y;

			var geometry = new PathGeometry(_renderTarget.Factory);
			var path = geometry.Open();
			path.BeginFigure(_point1, FigureBegin.Hollow);
			var arc = new ArcSegment
			{
				Point = _point2,
				Size = _size,
				SweepDirection = clockwise ? SweepDirection.Clockwise : SweepDirection.CounterClockwise,
				ArcSize = absRotation < 180 ? ArcSize.Small : ArcSize.Large
			};
			path.AddArc(arc);
			path.EndFigure(FigureEnd.Open);
			path.Close();

			// ReSharper disable once AccessToDisposedClosure
			Draw(ctx => ctx.DrawGeometry(geometry, CurrentState.DxStrokeBrush, strokeWidth, CurrentState.StrokeStyle));

			//geometry.Dispose();
		}


		public override void FillArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise)
		{
			while (startAngle < 0)
			{
				startAngle += 360;
			}

			while (endAngle < 0)
			{
				endAngle += 360;
			}

			var rotation = Geometry.GetSweep(startAngle, endAngle, clockwise);
			var absRotation = Math.Abs(rotation);

			_size.Width = width / 2;
			_size.Height = height / 2;

			var startPoint = Geometry.EllipseAngleToPoint(x, y, width, height, -startAngle);
			var endPoint = Geometry.EllipseAngleToPoint(x, y, width, height, -endAngle);

			_point1.X = startPoint.X;
			_point1.Y = startPoint.Y;

			_point2.X = endPoint.X;
			_point2.Y = endPoint.Y;

			var geometry = new PathGeometry(_renderTarget.Factory);
			var path = geometry.Open();
			path.BeginFigure(_point1, FigureBegin.Filled);
			var arc = new ArcSegment
			{
				Point = _point2,
				Size = _size,
				SweepDirection = clockwise ? SweepDirection.Clockwise : SweepDirection.CounterClockwise,
				ArcSize = absRotation < 180 ? ArcSize.Small : ArcSize.Large
			};
			path.AddArc(arc);
			path.EndFigure(FigureEnd.Closed);
			path.Close();

			// ReSharper disable once AccessToDisposedClosure
			Draw(ctx => ctx.FillGeometry(geometry, CurrentState.DxFillBrush));

			geometry.Dispose();
		}

		protected override void PlatformDrawRectangle(float x, float y, float width, float height)
		{
			float strokeWidth = CurrentState.StrokeSize;
			SetRect(x, y, width, height);
			Draw(ctx => ctx.DrawRectangle(_rect, CurrentState.DxStrokeBrush, strokeWidth, CurrentState.StrokeStyle));
		}

		protected override void PlatformDrawRoundedRectangle(float x, float y, float width, float height, float cornerRadius)
		{
			float strokeWidth = CurrentState.StrokeSize;
			SetRect(x, y, width, height);

			if (cornerRadius > _rect.Width / 2)
			{
				cornerRadius = _rect.Width / 2;
			}

			if (cornerRadius > _rect.Height / 2)
			{
				cornerRadius = _rect.Height / 2;
			}

			_roundedRect.Rect = _rect;
			_roundedRect.RadiusX = cornerRadius;
			_roundedRect.RadiusY = cornerRadius;

			Draw(ctx => ctx.DrawRoundedRectangle(_roundedRect, CurrentState.DxStrokeBrush, strokeWidth, CurrentState.StrokeStyle));
		}

		protected override void PlatformDrawEllipse(float x, float y, float width, float height)
		{
			float strokeWidth = CurrentState.StrokeSize;

			if (width > 0 || width < 0)
			{
				_point1.X = x + width / 2;
				_ellipse.RadiusX = width / 2;
			}
			else
			{
				_point1.X = x;
				_ellipse.RadiusX = 0;
			}

			if (height > 0 || height < 0)
			{
				_point1.Y = y + height / 2;
				_ellipse.RadiusY = height / 2;
			}
			else
			{
				_point1.Y = x;
				_ellipse.RadiusY = 0;
			}

			_ellipse.Point = _point1;

			Draw(ctx => ctx.DrawEllipse(_ellipse, CurrentState.DxStrokeBrush, strokeWidth, CurrentState.StrokeStyle));
		}

		private PathGeometry GetPath(PathF path, FillMode fillMode = FillMode.Winding)
		{
			var geometry = path.PlatformPath as PathGeometry;

			if (geometry == null)
			{
				geometry = path.AsDxPath(_renderTarget.Factory, fillMode);
				path.PlatformPath = geometry;
			}

			return geometry;
		}

		protected override void PlatformDrawPath(PathF path)
		{
			if (path == null)
				return;

			var geometry = GetPath(path);

			Draw(context =>
			{
// ReSharper disable AccessToDisposedClosure
				float strokeWidth = CurrentState.StrokeSize;
				context.DrawGeometry(geometry, CurrentState.DxStrokeBrush, strokeWidth, CurrentState.StrokeStyle);
			});
		}

		public override void ClipPath(PathF path, WindingMode windingMode = WindingMode.NonZero)
		{
			CurrentState.ClipPath(path, windingMode);
		}

		public override void ClipRectangle(float x, float y, float width, float height)
		{
			CurrentState.ClipRectangle(x, y, width, height);
		}

		public override void SubtractFromClip(EWPath path, float ppu)
		{    
			if (path == null)        
				return;        
			
			CurrentState.SubtractFromClip(path, ppu);
		}

		public override void SubtractFromClip(float x, float y, float width, float height)
		{
			CurrentState.SubtractFromClip(x, y, width, height);
		}

		public override void FillRectangle(float x, float y, float width, float height)
		{
			SetRect(x, y, width, height);

			Draw(ctx => ctx.FillRectangle(_rect, CurrentState.DxFillBrush));
		}

		public override void FillRoundedRectangle(float x, float y, float width, float height, float cornerRadius)
		{
			SetRect(x, y, width, height);

			if (cornerRadius > _rect.Width / 2)
			{
				cornerRadius = _rect.Width / 2;
			}

			if (cornerRadius > _rect.Height / 2)
			{
				cornerRadius = _rect.Height / 2;
			}

			_roundedRect.Rect = _rect;
			_roundedRect.RadiusX = cornerRadius;
			_roundedRect.RadiusY = cornerRadius;

			Draw(ctx => ctx.FillRoundedRectangle(_roundedRect, CurrentState.DxFillBrush));
		}

		public override void FillEllipse(float x, float y, float width, float height)
		{
			if (width > 0 || width < 0)
			{
				_point1.X = x + width / 2;
				_ellipse.RadiusX = width / 2;
			}
			else
			{
				_point1.X = x;
				_ellipse.RadiusX = 0;
			}

			if (height > 0 || height < 0)
			{
				_point1.Y = y + height / 2;
				_ellipse.RadiusY = height / 2;
			}
			else
			{
				_point1.Y = x;
				_ellipse.RadiusY = 0;
			}

			_ellipse.Point = _point1;

			Draw(ctx => ctx.FillEllipse(_ellipse, CurrentState.DxFillBrush));
		}

		public override void FillPath(PathF path, WindingMode windingMode)
		{
			var geometry = GetPath(path, windingMode == WindingMode.NonZero ? FillMode.Winding : FillMode.Alternate);
			Draw(ctx => ctx.FillGeometry(geometry, CurrentState.DxFillBrush));
		}

		public override void DrawString(string value, float x, float y, HorizontalAlignment horizontalAlignment)
		{
			// Initialize a TextFormat
#if DEBUG
			try
			{
#endif
				var textFormat = new TextFormat(
					FactoryDirectWrite,
					CurrentState.Font.Name,
					CurrentState.NativeFontWeight,
					CurrentState.NativeFontStyle,
					CurrentState.FontSize);

				if (horizontalAlignment == HorizontalAlignment.Left)
				{
					_rect.Left = x;
					_rect.Right = x + _renderTarget.PixelSize.Width;
					textFormat.TextAlignment = TextAlignment.Leading;
				}
				else if (horizontalAlignment == HorizontalAlignment.Right)
				{
					_rect.Right = x;
					_rect.Left = x - _renderTarget.PixelSize.Width;
					textFormat.TextAlignment = TextAlignment.Trailing;
				}
				else
				{
					_rect.Left = x - _renderTarget.PixelSize.Width;
					_rect.Right = x + _renderTarget.PixelSize.Width;
					textFormat.TextAlignment = TextAlignment.Center;
				}

				_rect.Top = y - CurrentState.FontSize;
				_rect.Bottom = y;

				// ReSharper disable AccessToDisposedClosure
				Draw(ctx => ctx.DrawText(value, textFormat, _rect, CurrentState.FontBrush, DrawTextOptions.None));
				// ReSharper restore AccessToDisposedClosure

				//textFormat.Dispose();

#if DEBUG
			}
			catch (Exception exc)
			{
				System.Diagnostics.Debug.WriteLine(exc);
			}
#endif
		}

		public override void DrawString(
			string value,
			float x,
			float y,
			float width,
			float height,
			HorizontalAlignment horizontalAlignment,
			VerticalAlignment verticalAlignment,
			TextFlow textFlow = TextFlow.ClipBounds,
			float lineAdjustment = 0)
		{
			// Initialize a TextFormat
			var textFormat = new TextFormat(FactoryDirectWrite, CurrentState.Font.Name,
				CurrentState.NativeFontWeight, CurrentState.NativeFontStyle, CurrentState.FontSize);

			switch (horizontalAlignment)
			{
				case HorizontalAlignment.Left:
					textFormat.TextAlignment = TextAlignment.Leading;
					break;
				case HorizontalAlignment.Center:
					textFormat.TextAlignment = TextAlignment.Center;
					break;
				case HorizontalAlignment.Right:
					textFormat.TextAlignment = TextAlignment.Trailing;
					break;
				case HorizontalAlignment.Justified:
					textFormat.TextAlignment = TextAlignment.Justified;
					break;
			}

			switch (verticalAlignment)
			{
				case VerticalAlignment.Top:
					textFormat.ParagraphAlignment = ParagraphAlignment.Near;
					break;
				case VerticalAlignment.Center:
					textFormat.ParagraphAlignment = ParagraphAlignment.Center;
					break;
				case VerticalAlignment.Bottom:
					textFormat.ParagraphAlignment = ParagraphAlignment.Far;
					break;
			}

			var options = DrawTextOptions.Clip;
			if (textFlow == TextFlow.OverflowBounds)
				options = DrawTextOptions.None;

			// Initialize a TextLayout
			var textLayout = new TextLayout(FactoryDirectWrite, value, textFormat,
				width, height);

			_point1.X = x;
			_point1.Y = y;

			// Draw the TextLayout
// ReSharper disable AccessToDisposedClosure
			Draw(ctx => ctx.DrawTextLayout(_point1, textLayout, CurrentState.FontBrush, options));
// ReSharper restore AccessToDisposedClosure

			//textLayout.Dispose();
			//textFormat.Dispose();
		}

		private readonly List<IDisposable> _drawTextDisposables = new List<IDisposable>();

		public override void DrawText(IAttributedText value, float x, float y, float width, float height)
		{
			if (value == null)
				return;

			var options = DrawTextOptions.Clip;

			var textFormat = new TextFormat(
				FactoryDirectWrite,
				CurrentState.Font.Name,
				CurrentState.NativeFontWeight,
				CurrentState.NativeFontStyle,
				CurrentState.FontSize);

			var textLayout = new TextLayout(FactoryDirectWrite, value.Text, textFormat, width, height);

			foreach (var run in value.Runs)
			{
				var range = new TextRange(run.Start, run.Length);
				TextBrush textBrush = null;
				foreach (var runAttribute in run.Attributes)
				{
					var key = runAttribute.Key;
					var attrValue = runAttribute.Value;

					switch (key)
					{
						case TextAttribute.Background:
							var colors = attrValue.Parse() ?? new[] {0f, 0f, 0f, 1f};
							if (textBrush == null)
							{
								textBrush = new TextBrush(_renderTarget, CurrentState.FontColor.AsDxColor());
								_drawTextDisposables.Add(textBrush);
								textLayout.SetDrawingEffect(textBrush, range);
							}

							textBrush.Background = new SolidColorBrush(_renderTarget, new Color4(colors[0], colors[1], colors[2], colors[3]));
							break;
						case TextAttribute.Bold:
							if ("True".Equals(attrValue))
								textLayout.SetFontWeight(FontWeight.Bold, range);
							break;
						case TextAttribute.Color:
							var textColor = attrValue.Parse() ?? new[] {0f, 0f, 0f, 1f};
							if (textBrush == null)
							{
								textBrush = new TextBrush(_renderTarget, new Color4(textColor[0], textColor[1], textColor[2], textColor[3]));
								_drawTextDisposables.Add(textBrush);
								textLayout.SetDrawingEffect(textBrush, range);
							}
							else
							{
								textBrush.Color = new Color4(textColor[0], textColor[1], textColor[2], textColor[3]);
							}

							break;
						case TextAttribute.FontName:
							var fontName = attrValue ?? Graphics.Font.Default.Name;
							textLayout.SetFontFamilyName(fontName, range);
							break;
						case TextAttribute.FontSize:
							if ("True".Equals(attrValue))
							{
								if (!float.TryParse(attrValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var fontSize))
									fontSize = CurrentState.FontSize;
								textLayout.SetFontSize(fontSize, range);
							}

							break;
						case TextAttribute.Italic:
							if ("True".Equals(attrValue))
								textLayout.SetFontStyle(FontStyle.Italic, range);
							break;
						case TextAttribute.Strikethrough:
							if ("True".Equals(attrValue))
								textLayout.SetStrikethrough(new RawBool(true), range);
							break;
						case TextAttribute.Subscript:
							/* todo: add support for subscript */
							break;
						case TextAttribute.Superscript:
							/* todo: add support for superscripts */
							break;
						case TextAttribute.Underline:
							if ("True".Equals(attrValue))
								textLayout.SetUnderline(new RawBool(true), range);
							break;
					}
				}
			}

			_point1.X = x;
			_point1.Y = y;

			Draw(ctx => CustomBrushTextRenderer.DrawTextLayout(ctx, _point1, textLayout, CurrentState.FontBrush, options));

			foreach (var disposable in _drawTextDisposables)
				disposable.Dispose();

			_drawTextDisposables.Clear();
		}

		protected override void PlatformRotate(float degrees, float radians, float x, float y)
		{
			_renderTarget.Transform = CurrentState.DxRotate(degrees, x, y);
		}

		protected override void PlatformRotate(float degrees, float radians)
		{
			_renderTarget.Transform = CurrentState.DxRotate(degrees);
		}

		protected override void PlatformScale(float sx, float sy)
		{
			_renderTarget.Transform = CurrentState.DxScale(sx, sy);
		}

		protected override void PlatformTranslate(float tx, float ty)
		{
			_renderTarget.Transform = CurrentState.DxTranslate(tx, ty);
		}

		protected override void PlatformConcatenateTransform(NumericsMatrix3x2 transform)
		{
			_renderTarget.Transform = CurrentState.DxConcatenateTransform(transform);
		}

		public override void SaveState()
		{
			CurrentState.SaveRenderTargetState();
			base.SaveState();
		}

		protected override void StateRestored(DXCanvasState state)
		{
			if (_renderTarget != null)
				state?.RestoreRenderTargetState();
		}

		public override void SetShadow(SizeF offset, float blur, Color color)
		{
			CurrentState.SetShadow(offset, blur, color);
		}

		protected override void PlatformSetStrokeDashPattern(float[] strokePattern, float strokeDashOffset, float strokeSize)
		{
			CurrentState.SetStrokeDashPattern(strokePattern, strokeDashOffset, strokeSize);
		}

		protected override float PlatformStrokeSize
		{
			set { }
		}

		public override void SetFillPaint(Paint paint, RectF rectangle)
		{
			if (paint == null)
			{
				CurrentState.FillColor = Colors.White;
				return;
			}

			if (paint is SolidPaint solidPaint)
			{
				CurrentState.FillColor = solidPaint.Color;
				return;
			}

			if (paint is ImagePaint imagePaint)
			{
				if (imagePaint.Image is DXImage image)
				{
					var parentContext = _renderTarget as DeviceContext;
					var properties = new BitmapBrushProperties
					{
						ExtendModeX = ExtendMode.Wrap,
						ExtendModeY = ExtendMode.Wrap
					};
					var bitmapBrush = new BitmapBrush(parentContext, image.NativeImage, properties);
					CurrentState.SetBitmapBrush(bitmapBrush);
				}
				else
				{
					CurrentState.FillColor = Colors.White;
				}

				return;
			}

			if (paint is PatternPaint patternPaint)
			{
				var parentContext = _renderTarget as DeviceContext;
				var pattern = patternPaint.Pattern;
				if (pattern == null)
				{
					CurrentState.FillColor = Colors.White;
					return;
				}

				if (_bitmapPatternFills)
				{
					var bitmap = CreatePatternBitmap(pattern);
					if (bitmap != null)
					{
						var properties = new BitmapBrushProperties1
						{
							ExtendModeX = ExtendMode.Wrap,
							ExtendModeY = ExtendMode.Wrap
						};
						var bitmapBrush = new BitmapBrush1(parentContext, bitmap, properties);
						CurrentState.SetBitmapBrush(bitmapBrush);
					}
					else
					{
						CurrentState.FillColor = Colors.White;
					}
				}
				else
				{
					var commandList = CreatePatternCommandList(pattern);
					if (commandList != null)
					{
						var properties = new ImageBrushProperties
						{
							ExtendModeX = ExtendMode.Wrap,
							ExtendModeY = ExtendMode.Wrap,
							SourceRectangle =
								new global::SharpDX.RectangleF(
									(pattern.Width - pattern.StepX) / 2, (pattern.Height - pattern.StepY) / 2,
									pattern.StepX, pattern.StepY)
						};
						var imageBrush = new ImageBrush(parentContext, commandList, properties);
						CurrentState.SetBitmapBrush(imageBrush);
					}
					else
					{
						CurrentState.FillColor = Colors.White;
					}
				}

				return;
			}

			_fillRectangle = rectangle;

			if (paint is LinearGradientPaint linearGradientPaint)
			{
				float x1 = (float)(linearGradientPaint.StartPoint.X * rectangle.Width) + rectangle.X;
				float y1 = (float)(linearGradientPaint.StartPoint.Y * rectangle.Height) + rectangle.Y;

				float x2 = (float)(linearGradientPaint.EndPoint.X * rectangle.Width) + rectangle.X;
				float y2 = (float)(linearGradientPaint.EndPoint.Y * rectangle.Height) + rectangle.Y;

				_point1.X = x1;
				_point1.Y = y1;
				_point2.X = x2;
				_point2.Y = y2;

				CurrentState.SetLinearGradient(paint, _point1, _point2);
			}
			else if (paint is RadialGradientPaint radialGradientPaint)
			{
				float centerX = (float)(radialGradientPaint.Center.X * rectangle.Width) + rectangle.X;
				float centerY = (float)(radialGradientPaint.Center.Y * rectangle.Height) + rectangle.Y;
				float radius = (float)radialGradientPaint.Radius * Math.Max(rectangle.Height, rectangle.Width);

				_point1.X = centerX;
				_point1.Y = centerY;
				_radius = radius;

				CurrentState.SetRadialGradient(paint, _point1, _radius, _fillRectangle);
			}
		}

		private Bitmap1 CreatePatternBitmap(IPattern pattern)
		{
			var context = GetOrCreatePatternContext(new Size2((int) pattern.Width, (int) pattern.Height));
			if (context != null)
			{
				context.BeginDraw();
				context.Clear(global::SharpDX.Color.Transparent);
				var canvas = new DXCanvas(context);
				pattern.Draw(canvas);
				context.EndDraw();

				return _patternBitmap;
			}

			return null;
		}

		private CommandList CreatePatternCommandList(IPattern pattern)
		{
			var context = GetOrCreateVectorContext();
			if (context != null)
			{
				var commandList = new CommandList(_renderTarget as DeviceContext);
				context.Target = commandList;
				context.BeginDraw();
				var canvas = new DXCanvas(context);

				pattern.Draw(canvas);

				context.EndDraw();
				commandList.Close();

				return commandList;
			}

			return null;
		}

		private DeviceContext GetOrCreateVectorContext()
		{
			if (_vectorPatternContext != null)
			{
				return _vectorPatternContext;
			}

			if (_renderTarget is DeviceContext parentContext)
			{
				if (_vectorPatternContext == null)
				{
					// initialize the DeviceContext - it will be the D2D render target
					_vectorPatternContext = new DeviceContext(parentContext.Device, DeviceContextOptions.None);
				}
			}

			return _vectorPatternContext;
		}

		public void DrawBitmap(Bitmap bitmap, global::SharpDX.RectangleF targetRect, float opacity)
		{
			Draw(ctx => ctx.DrawBitmap(bitmap, targetRect, opacity, BitmapInterpolationMode.Linear));
		}

		public override void DrawImage(IImage image, float x, float y, float width, float height)
		{
			SetRect(x, y, width, height);
			var bitmap = image.AsBitmap();
			if (bitmap != null)
			{
				Draw(ctx => ctx.DrawBitmap(bitmap, _rect, CurrentState.Alpha, BitmapInterpolationMode.Linear));
			}
#if DEBUG
			else
			{
				System.Diagnostics.Debug.WriteLine("Unable to draw the image because a bitmap could not be created.");
			}
#endif
		}

		private void SetRect(float x, float y, float width, float height)
		{
			_rect.Left = Math.Min(x, x + width);
			_rect.Top = Math.Min(y, y + height);
			_rect.Right = Math.Max(x, x + width);
			_rect.Bottom = Math.Max(y, y + height);
		}

		private DeviceContext GetOrCreatePatternContext(Size2 patternSize)
		{
			if (_patternContext != null)
			{
				// If the effect bitmap size does not equal the size of our render target, then dispose of it
				// and create a new one.
				if (_patternBitmap.PixelSize != patternSize)
				{
					//patternBitmap.Dispose();
					_patternBitmap = null;
				}
				else
				{
					return _patternContext;
				}
			}

			if (_renderTarget is DeviceContext parentContext)
			{
				if (_patternContext == null)
				{
					// initialize the DeviceContext - it will be the D2D render target
					_patternContext = new DeviceContext(parentContext.Device, DeviceContextOptions.None);
				}

				// specify a pixel format that is supported by both and WIC
				var pixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);

				// create the d2d bitmap description
				var bitmapProperties = new BitmapProperties1(pixelFormat, Dpi, Dpi, BitmapOptions.Target);

				// create our d2d bitmap where all drawing will happen
				_patternBitmap = new Bitmap1(_effectContext, patternSize, bitmapProperties);

				// associate bitmap with the d2d context
				_patternContext.Target = _patternBitmap;
			}

			return _patternContext;
		}

		private DeviceContext GetOrCreateEffectContext()
		{
			if (_effectContext != null)
			{
				// If the effect bitmap size does not equal the size of our render target, then dispose of it
				// and create a new one.
				if (_effectBitmap.PixelSize != _renderTarget.PixelSize)
				{
					//effectBitmap.Dispose();
					_effectBitmap = null;

					if (_shadowEffect != null)
					{
						// shadowEffect.Dispose();
						_shadowEffect = null;
					}

					if (_blurEffect != null)
					{
						//blurEffect.Dispose();
						_blurEffect = null;
					}
				}
				else
				{
					return _effectContext;
				}
			}

			if (_renderTarget is DeviceContext parentContext)
			{
				if (_effectContext == null)
				{
					// initialize the DeviceContext - it will be the D2D render target
					_effectContext = new DeviceContext(parentContext.Device, DeviceContextOptions.None);
				}

				// specify a pixel format that is supported by both and WIC
				var pixelFormat = new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);

				// create the d2d bitmap description
				var bitmapProperties = new BitmapProperties1(pixelFormat, Dpi, Dpi, BitmapOptions.Target);

				// create our d2d bitmap where all drawing will happen
				_effectBitmap = new Bitmap1(_effectContext, _renderTarget.PixelSize, bitmapProperties);

				// associate bitmap with the d2d context
				_effectContext.Target = _effectBitmap;
			}

			return _effectContext;
		}

		private void DrawShadow(Action<DeviceContext> drawingAction)
		{
			var context = GetOrCreateEffectContext();
			if (context != null)
			{
				context.BeginDraw();
				context.Transform = CurrentState.Matrix.Translate(CurrentState.ShadowOffset.X, CurrentState.ShadowOffset.Y);
				context.Clear(global::SharpDX.Color.Transparent);
				drawingAction(context);
				context.EndDraw();

				if (_shadowEffect == null)
				{
					_shadowEffect = new Shadow((DeviceContext) _renderTarget);
					_shadowEffect.SetInput(0, _effectBitmap, new RawBool(false));
				}

				_shadowEffect.BlurStandardDeviation = CurrentState.ActualShadowBlur / 3;
				_shadowEffect.Color = CurrentState.ShadowColor;

				_renderTarget.Transform = Matrix3x2.Identity;
				((DeviceContext) _renderTarget).DrawImage(_shadowEffect);
				_renderTarget.Transform = CurrentState.Matrix;
			}
		}

		private void DrawBlurred(Action<DeviceContext> drawingAction)
		{
			var context = GetOrCreateEffectContext();
			if (context != null)
			{
				context.BeginDraw();
				context.Transform = CurrentState.Matrix;
				context.Clear(global::SharpDX.Color.Transparent);
				drawingAction(context);
				context.EndDraw();

				if (_blurEffect == null)
				{
					_blurEffect = new GaussianBlur((DeviceContext) _renderTarget);
					_blurEffect.SetInput(0, _effectBitmap, new RawBool(false));
					_blurEffect.BorderMode = BorderMode.Soft;
					_blurEffect.Optimization = GaussianBlurOptimization.Speed;
				}

				_blurEffect.StandardDeviation = CurrentState.BlurRadius / 3;

				_renderTarget.Transform = Matrix3x2.Identity;
				((DeviceContext) _renderTarget).DrawImage(_blurEffect);
				_renderTarget.Transform = CurrentState.Matrix;
			}
		}

		private void Draw(Action<DeviceContext> drawingAction)
		{
			if (CurrentState.IsShadowed)
			{
				DrawShadow(drawingAction);
			}

			if (CurrentState.IsBlurred)
			{
				DrawBlurred(drawingAction);
			}
			else
			{
				drawingAction((DeviceContext) _renderTarget);
			}
		}

		public override SizeF GetStringSize(string value, IFont font, float textSize)
		{
			if (value == null) return new SizeF();

			float fontSize = textSize;
			float factor = 1;
			while (fontSize > 14)
			{
				fontSize /= 14;
				factor *= 14;
			}

			if (font == null)
				font = Graphics.Font.Default;

			var size = new SizeF();

			var textFormat = new TextFormat(FactoryDirectWrite, font.Name, fontSize);
			textFormat.TextAlignment = TextAlignment.Leading;
			textFormat.ParagraphAlignment = ParagraphAlignment.Near;

			var textLayout = new TextLayout(FactoryDirectWrite, value, textFormat, 512, 512);
			size.Width = textLayout.Metrics.Width;
			size.Height = textLayout.Metrics.Height;

			size.Width *= factor;
			size.Height *= factor;

			return size;
		}

		public override SizeF GetStringSize(
			string value,
			IFont font,
			float textSize,
			HorizontalAlignment horizontalAlignment,
			VerticalAlignment verticalAlignment)
		{
			if (value == null) return new SizeF();

			float fontSize = textSize;
			float factor = 1;
			while (fontSize > 14)
			{
				fontSize /= 14;
				factor *= 14;
			}

			var size = new SizeF();

			var textFormat = new TextFormat(FactoryDirectWrite, font.Name, font.Weight.ToFontWeight(), font.StyleType.ToFontStyle(), fontSize);
			if (horizontalAlignment == HorizontalAlignment.Left)
			{
				textFormat.TextAlignment = TextAlignment.Leading;
			}
			else if (horizontalAlignment == HorizontalAlignment.Center)
			{
				textFormat.TextAlignment = TextAlignment.Center;
			}
			else if (horizontalAlignment == HorizontalAlignment.Right)
			{
				textFormat.TextAlignment = TextAlignment.Trailing;
			}
			else if (horizontalAlignment == HorizontalAlignment.Justified)
			{
				textFormat.TextAlignment = TextAlignment.Justified;
			}

			if (verticalAlignment == VerticalAlignment.Top)
			{
				textFormat.ParagraphAlignment = ParagraphAlignment.Near;
			}
			else if (verticalAlignment == VerticalAlignment.Center)
			{
				textFormat.ParagraphAlignment = ParagraphAlignment.Center;
			}
			else if (verticalAlignment == VerticalAlignment.Bottom)
			{
				textFormat.ParagraphAlignment = ParagraphAlignment.Far;
			}

			var textLayout = new TextLayout(FactoryDirectWrite, value, textFormat, 512f, 512f, Dip, false);
			size.Width = textLayout.Metrics.Width;
			size.Height = textLayout.Metrics.Height;


			size.Width *= factor;
			size.Height *= factor;

			return size;
		}
	}
}
