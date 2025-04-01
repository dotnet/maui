using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Maui.Graphics.Text;
using System;
using System.Numerics;
using WRect = Windows.Foundation.Rect;
#if NETFX_CORE
using WColors = global::Windows.UI.Colors;
#else
using WColors = global::Microsoft.UI.Colors;
#endif

#if MAUI_GRAPHICS_WIN2D
namespace Microsoft.Maui.Graphics.Win2D
#else
namespace Microsoft.Maui.Graphics.Platform
#endif
{
	/// <summary>
	/// A Windows platform implementation of <see cref="AbstractCanvas{T}"/>.
	/// </summary>
#if MAUI_GRAPHICS_WIN2D
	public class W2DCanvas
#else
	public class PlatformCanvas
#endif
		: AbstractCanvas<PlatformCanvasState>, IBlurrableCanvas
	{
		private CanvasDrawingSession _session;
		private CanvasRenderTarget _patternContext;
		private CanvasRenderTarget _effectContext;
		private ShadowEffect _shadowEffect;
		private GaussianBlurEffect _blurEffect;
		private global::Windows.Foundation.Size _canvasSize;
		private Vector2 _point1;
		private Vector2 _point2;
		private Vector2 _linearGradientStartPoint;
		private Vector2 _linearGradientEndPoint;
		private Vector2 _radialGradientCenter;
		private float _radialGradientRadius;

		/* Unmerged change from project 'Microsoft.Maui.Graphics.Win2D.WinUI.Desktop'
		Before:
				private Rect _rect;
		After:
				private global::Windows.Foundation.Rect _rect;
		*/
		private WRect _rect;
		private global::Windows.Foundation.Size _size;

		private bool _bitmapPatternFills;

#if MAUI_GRAPHICS_WIN2D
		public W2DCanvas()
#else
		public PlatformCanvas()
#endif
			: base(new PlatformCanvasStateService(), new PlatformStringSizeService())
		{
		}

		public CanvasDrawingSession Session
		{
			get => _session;
			set => _session = value;
		}

		public global::Windows.Foundation.Size CanvasSize
		{
			get => _canvasSize;
			set => _canvasSize = value;
		}

		public bool BitmapPatternFills
		{
			get => _bitmapPatternFills;
			set => _bitmapPatternFills = value;
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

		protected override void PlatformSetStrokeDashPattern(float[] strokePattern, float strokeDashOffset, float strokeSize)
		{
			CurrentState.SetStrokeDashPattern(strokePattern, strokeDashOffset, strokeSize);
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
			set => CurrentState.Font = value;
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
			set => _session.Antialiasing = value ? CanvasAntialiasing.Antialiased : CanvasAntialiasing.Aliased;
		}

		public override BlendMode BlendMode
		{
			set { }
		}

		public override void FillPath(PathF path, WindingMode windingMode)
		{
			var geometry = GetPath(path, windingMode == WindingMode.NonZero ? CanvasFilledRegionDetermination.Winding : CanvasFilledRegionDetermination.Alternate);
			Draw(s => s.FillGeometry(geometry, CurrentState.PlatformFillBrush));
		}


		public override void SubtractFromClip(float x, float y, float width, float height)
		{
			CurrentState.SubtractFromClip(x, y, width, height);
		}

		public override void ClipPath(PathF path, WindingMode windingMode = WindingMode.NonZero)
		{
			CurrentState.ClipPath(path, windingMode);
		}

		public override void ClipRectangle(float x, float y, float width, float height)
		{
			CurrentState.ClipRectangle(x, y, width, height);
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

			var rotation = GeometryUtil.GetSweep(startAngle, endAngle, clockwise);
			var absRotation = Math.Abs(rotation);

			float strokeWidth = CurrentState.StrokeSize;
			SetRect(x, y, width, height);

			_size.Width = _rect.Width / 2;
			_size.Height = _rect.Height / 2;

			var startPoint = GeometryUtil.EllipseAngleToPoint((float)_rect.X, (float)_rect.Y, (float)_rect.Width, (float)_rect.Height, -startAngle);
			var endPoint = GeometryUtil.EllipseAngleToPoint((float)_rect.X, (float)_rect.Y, (float)_rect.Width, (float)_rect.Height, -endAngle);

			_point1.X = startPoint.X;
			_point1.Y = startPoint.Y;

			_point2.X = endPoint.X;
			_point2.Y = endPoint.Y;

			var builder = new CanvasPathBuilder(_session);
			builder.BeginFigure(_point1, CanvasFigureFill.Default);
			builder.AddArc(
				new Vector2(_point2.X, _point2.Y),
				(float)_size.Width,
				(float)_size.Height,
				0,
				clockwise ? CanvasSweepDirection.Clockwise : CanvasSweepDirection.CounterClockwise,
				absRotation >= 180 ? CanvasArcSize.Large : CanvasArcSize.Small);

			builder.EndFigure(CanvasFigureLoop.Closed);
			var geometry = CanvasGeometry.CreatePath(builder);

			Draw(ctx => ctx.FillGeometry(geometry, CurrentState.PlatformFillBrush));
		}

		public override void FillRectangle(float x, float y, float width, float height)
		{
			Draw(s => s.FillRectangle(x, y, width, height, CurrentState.PlatformFillBrush));
		}

		public override void FillRoundedRectangle(float x, float y, float width, float height, float cornerRadius)
		{
			Draw(s => s.FillRoundedRectangle(x, y, width, height, cornerRadius, cornerRadius, CurrentState.PlatformFillBrush));
		}

		public override void FillEllipse(float x, float y, float width, float height)
		{
			float radiusX;
			float radiusY;

			if (width > 0 || width < 0)
			{
				_point1.X = x + width / 2;
				radiusX = width / 2;
			}
			else
			{
				_point1.X = x;
				radiusX = 0;
			}

			if (height > 0 || height < 0)
			{
				_point1.Y = y + height / 2;
				radiusY = height / 2;
			}
			else
			{
				_point1.Y = x;
				radiusY = 0;
			}

			Draw(s => s.FillEllipse(_point1, radiusX, radiusY, CurrentState.PlatformFillBrush));
		}

		public override void DrawString(string value, float x, float y, HorizontalAlignment horizontalAlignment)
		{
			// Initialize a TextFormat
#if DEBUG
			try
			{
#endif
			var textFormat = (CurrentState.Font ?? Graphics.Font.Default).ToCanvasTextFormat(CurrentState.FontSize);
			textFormat.VerticalAlignment = CanvasVerticalAlignment.Top;

			switch (horizontalAlignment)
			{
				case HorizontalAlignment.Left:
					_rect.X = x;
					_rect.Width = CanvasSize.Width;
					textFormat.HorizontalAlignment = CanvasHorizontalAlignment.Left;
					break;
				case HorizontalAlignment.Right:
					_rect.X = x - CanvasSize.Width;
					_rect.Width = CanvasSize.Width;
					textFormat.HorizontalAlignment = CanvasHorizontalAlignment.Right;
					break;
				default:
					_rect.X = x - _canvasSize.Width;
					_rect.Width = _canvasSize.Width * 2;
					textFormat.HorizontalAlignment = CanvasHorizontalAlignment.Center;
					break;
			}

			_rect.Y = y - CurrentState.FontSize;
			_rect.Height = CurrentState.FontSize * 2;

			_point1.X = (float)_rect.X;
			_point1.Y = (float)_rect.Y;

			var textLayout = new CanvasTextLayout(
				_session,
				value,
				textFormat,
				(float)_rect.Width,
				(float)_rect.Height);

			Draw(ctx => ctx.DrawTextLayout(textLayout, _point1, CurrentState.PlatformFontBrush));

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
			var textFormat = (CurrentState.Font ?? Graphics.Font.Default).ToCanvasTextFormat(CurrentState.FontSize);

			switch (horizontalAlignment)
			{
				case HorizontalAlignment.Left:
					textFormat.HorizontalAlignment = CanvasHorizontalAlignment.Left;
					break;
				case HorizontalAlignment.Center:
					textFormat.HorizontalAlignment = CanvasHorizontalAlignment.Center;
					break;
				case HorizontalAlignment.Right:
					textFormat.HorizontalAlignment = CanvasHorizontalAlignment.Right;
					break;
				case HorizontalAlignment.Justified:
					textFormat.HorizontalAlignment = CanvasHorizontalAlignment.Justified;
					break;
			}

			switch (verticalAlignment)
			{
				case VerticalAlignment.Top:
					textFormat.VerticalAlignment = CanvasVerticalAlignment.Top;
					break;
				case VerticalAlignment.Center:
					textFormat.VerticalAlignment = CanvasVerticalAlignment.Center;
					break;
				case VerticalAlignment.Bottom:
					textFormat.VerticalAlignment = CanvasVerticalAlignment.Bottom;
					break;
			}

			// Initialize a TextLayout
			var textLayout = new CanvasTextLayout(
				_session,
				value,
				textFormat,
				width,
				height)
			{
				Options = textFlow == TextFlow.OverflowBounds
					? CanvasDrawTextOptions.Default
					: CanvasDrawTextOptions.Clip
			};


			_point1.X = x;
			_point1.Y = y;

			Draw(ctx => ctx.DrawTextLayout(textLayout, _point1, CurrentState.PlatformFontBrush));
		}

		public override void DrawText(IAttributedText value, float x, float y, float width, float height)
		{
			throw new NotImplementedException();
		}

		public override void SetShadow(SizeF offset, float blur, Color color)
		{
			CurrentState.SetShadow(offset, blur, color);
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
				if (imagePaint.Image is Platform.PlatformImage image)
				{
					var bitmapBrush = new CanvasImageBrush(_session, image.PlatformRepresentation)
					{
						ExtendX = CanvasEdgeBehavior.Wrap,
						ExtendY = CanvasEdgeBehavior.Wrap
					};
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
						var bitmapBrush = new CanvasImageBrush(_session, bitmap)
						{
							ExtendX = CanvasEdgeBehavior.Wrap,
							ExtendY = CanvasEdgeBehavior.Wrap
						};
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
						var imageBrush = new CanvasImageBrush(_session, commandList)
						{
							ExtendX = CanvasEdgeBehavior.Wrap,
							ExtendY = CanvasEdgeBehavior.Wrap,

							/* Unmerged change from project 'Microsoft.Maui.Graphics.Win2D.WinUI.Desktop'
							Before:
														SourceRectangle = new Rect(
							After:
														SourceRectangle = new global::Windows.Foundation.Rect(
							*/
							SourceRectangle = new WRect(
									(pattern.Width - pattern.StepX) / 2,
									(pattern.Height - pattern.StepY) / 2,
									pattern.StepX,
									pattern.StepY)
						};
						CurrentState.SetBitmapBrush(imageBrush);
					}
					else
					{
						CurrentState.FillColor = Colors.White;
					}
				}
				return;
			}

			if (paint is LinearGradientPaint linearGradientPaint)
			{
				float x1 = (float)(linearGradientPaint.StartPoint.X * rectangle.Width) + rectangle.X;
				float y1 = (float)(linearGradientPaint.StartPoint.Y * rectangle.Height) + rectangle.Y;

				float x2 = (float)(linearGradientPaint.EndPoint.X * rectangle.Width) + rectangle.X;
				float y2 = (float)(linearGradientPaint.EndPoint.Y * rectangle.Height) + rectangle.Y;

				_linearGradientStartPoint.X = x1;
				_linearGradientStartPoint.Y = y1;
				_linearGradientEndPoint.X = x2;
				_linearGradientEndPoint.Y = y2;

				CurrentState.SetLinearGradient(paint, _linearGradientStartPoint, _linearGradientEndPoint);
			}
			else if (paint is RadialGradientPaint radialGradientPaint)
			{
				float centerX = (float)(radialGradientPaint.Center.X * rectangle.Width) + rectangle.X;
				float centerY = (float)(radialGradientPaint.Center.Y * rectangle.Height) + rectangle.Y;
				float radius = (float)radialGradientPaint.Radius * Math.Max(rectangle.Height, rectangle.Width);

				_radialGradientCenter.X = centerX;
				_radialGradientCenter.Y = centerY;
				_radialGradientRadius = radius;

				CurrentState.SetRadialGradient(paint, _radialGradientCenter, _radialGradientRadius);
			}
		}

		private CanvasCommandList CreatePatternCommandList(IPattern pattern)
		{
			var commandList = new CanvasCommandList(_session);
			using (var patternSession = commandList.CreateDrawingSession())
			{
				var canvas = new PlatformCanvas { Session = patternSession };
				pattern?.Draw(canvas);
			}

			return commandList;
		}


		private CanvasBitmap CreatePatternBitmap(IPattern pattern)
		{
			var context = GetOrCreatePatternContext(new global::Windows.Foundation.Size(pattern.Width, pattern.Height));
			if (context != null)
			{
				using (var imageSession = context.CreateDrawingSession())
				{
					imageSession.Clear(WColors.Transparent);
					var canvas = new PlatformCanvas { Session = imageSession };
					pattern.Draw(canvas);
				}

				return context;
			}

			return null;
		}

		public override void DrawImage(IImage image, float x, float y, float width, float height)
		{
			if (image.ToPlatformImage() is Platform.PlatformImage platformImage)
			{
				SetRect(x, y, width, height);

				/* Unmerged change from project 'Microsoft.Maui.Graphics.Win2D.WinUI.Desktop'
				Before:
								Draw(s => s.DrawImage(platformImage.PlatformImage, _rect, Rect.Empty, CurrentState.Alpha, CanvasImageInterpolation.Linear));
				After:
								Draw(s => s.DrawImage(platformImage.PlatformImage, _rect, global::Windows.Foundation.Rect.Empty, CurrentState.Alpha, CanvasImageInterpolation.Linear));
				*/
				Draw(s => s.DrawImage(platformImage.PlatformRepresentation, _rect, WRect.Empty, CurrentState.Alpha, CanvasImageInterpolation.Linear));
			}
		}

		protected override float PlatformStrokeSize
		{
			set { }
		}

		protected override void PlatformDrawLine(float x1, float y1, float x2, float y2)
		{
			Draw(s => s.DrawLine(x1, y1, x2, y2, CurrentState.PlatformStrokeBrush, CurrentState.StrokeSize, CurrentState.PlatformStrokeStyle));
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

			var rotation = GeometryUtil.GetSweep(startAngle, endAngle, clockwise);
			var absRotation = Math.Abs(rotation);

			float strokeWidth = CurrentState.StrokeSize;
			SetRect(x, y, width, height);

			_size.Width = _rect.Width / 2;
			_size.Height = _rect.Height / 2;

			var startPoint = GeometryUtil.EllipseAngleToPoint((float)_rect.X, (float)_rect.Y, (float)_rect.Width, (float)_rect.Height, -startAngle);
			var endPoint = GeometryUtil.EllipseAngleToPoint((float)_rect.X, (float)_rect.Y, (float)_rect.Width, (float)_rect.Height, -endAngle);

			_point1.X = startPoint.X;
			_point1.Y = startPoint.Y;

			_point2.X = endPoint.X;
			_point2.Y = endPoint.Y;

			var builder = new CanvasPathBuilder(_session);
			builder.BeginFigure(_point1, CanvasFigureFill.Default);
			builder.AddArc(
				new Vector2(_point2.X, _point2.Y),
				(float)_size.Width,
				(float)_size.Height,
				0,
				clockwise ? CanvasSweepDirection.Clockwise : CanvasSweepDirection.CounterClockwise,
				absRotation >= 180 ? CanvasArcSize.Large : CanvasArcSize.Small);

			builder.EndFigure(CanvasFigureLoop.Open);
			var geometry = CanvasGeometry.CreatePath(builder);

			Draw(ctx => ctx.DrawGeometry(geometry, CurrentState.PlatformStrokeBrush, strokeWidth, CurrentState.PlatformStrokeStyle));
		}

		protected override void PlatformDrawRectangle(float x, float y, float width, float height)
		{
			float strokeWidth = CurrentState.StrokeSize;
			SetRect(x, y, width, height);

			Draw(s => s.DrawRectangle(_rect, CurrentState.PlatformStrokeBrush, CurrentState.StrokeSize, CurrentState.PlatformStrokeStyle));
		}

		protected override void PlatformDrawRoundedRectangle(float x, float y, float width, float height, float cornerRadius)
		{
			float strokeWidth = CurrentState.StrokeSize;
			SetRect(x, y, width, height);

			if (cornerRadius > _rect.Width / 2)
			{
				cornerRadius = (float)_rect.Width / 2;
			}

			if (cornerRadius > _rect.Height / 2)
			{
				cornerRadius = (float)_rect.Height / 2;
			}

			Draw(s => s.DrawRoundedRectangle(_rect, cornerRadius, cornerRadius, CurrentState.PlatformStrokeBrush, CurrentState.StrokeSize, CurrentState.PlatformStrokeStyle));
		}

		protected override void PlatformDrawEllipse(float x, float y, float width, float height)
		{
			float radiusX;
			float radiusY;
			float px;
			float py;
			float strokeWidth = CurrentState.StrokeSize;

			if (width > 0 || width < 0)
			{
				px = x + width / 2;
				radiusX = width / 2;
			}
			else
			{
				px = x;
				radiusX = 0;
			}

			if (height > 0 || height < 0)
			{
				py = y + height / 2;
				radiusY = height / 2;
			}
			else
			{
				py = x;
				radiusY = 0;
			}

			Draw(s => s.DrawEllipse(px, py, radiusX, radiusY, CurrentState.PlatformStrokeBrush, CurrentState.StrokeSize, CurrentState.PlatformStrokeStyle));
		}

		private CanvasGeometry GetPath(PathF path, CanvasFilledRegionDetermination fillMode = CanvasFilledRegionDetermination.Winding)
		{
			var geometry = path.PlatformPath as CanvasGeometry;

			if (geometry == null)
			{
				geometry = path.AsPath(_session, fillMode);
				path.PlatformPath = geometry;
			}

			return geometry;
		}

		protected override void PlatformDrawPath(PathF path)
		{
			if (path == null)
				return;

			var geometry = GetPath(path);

			Draw(s =>
			{
				// ReSharper disable AccessToDisposedClosure
				float strokeWidth = CurrentState.StrokeSize;
				s.DrawGeometry(geometry, CurrentState.PlatformStrokeBrush, strokeWidth, CurrentState.PlatformStrokeStyle);
			});
		}

		protected override void PlatformRotate(float degrees, float radians, float x, float y)
		{
			_session.Transform = CurrentState.AppendRotate(degrees, x, y);
		}

		protected override void PlatformRotate(float degrees, float radians)
		{
			_session.Transform = CurrentState.AppendRotate(degrees);
		}

		protected override void PlatformScale(float sx, float sy)
		{
			_session.Transform = CurrentState.AppendScale(sx, sy);
		}

		protected override void PlatformTranslate(float tx, float ty)
		{
			_session.Transform = CurrentState.AppendTranslate(tx, ty);
		}

		protected override void PlatformConcatenateTransform(Matrix3x2 transform)
		{
			_session.Transform = CurrentState.AppendConcatenateTransform(transform);
		}

		public void SetBlur(float blurRadius)
		{
			CurrentState.SetBlur(blurRadius);
		}

		public override void SaveState()
		{
			CurrentState.SaveRenderTargetState();
			base.SaveState();
		}

		protected override void StateRestored(PlatformCanvasState state)
		{
			if (_session != null)
			{
				state?.RestoreRenderTargetState();
			}
		}

		private void Draw(Action<CanvasDrawingSession> drawingAction)
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
				drawingAction(_session);
			}
		}

		private CanvasRenderTarget GetOrCreatePatternContext(global::Windows.Foundation.Size patternSize)
		{
			if (_patternContext != null)
			{
				// If the effect bitmap size does not equal the size of our render target, then dispose of it
				// and create a new one.
				if (_patternContext.Size != patternSize)
				{
					_patternContext.Dispose();
					_patternContext = null;
				}
				else
				{
					return _patternContext;
				}
			}

			_patternContext = new CanvasRenderTarget(_session, _canvasSize);
			return _patternContext;
		}

		private CanvasRenderTarget GetOrCreateEffectContext()
		{
			if (_effectContext != null)
			{
				// If the effect bitmap size does not equal the size of our render target, then dispose of it
				// and create a new one.
				if (_effectContext.Size != CanvasSize)
				{
					_effectContext.Dispose();
					_effectContext = null;

					_shadowEffect?.Dispose();
					_shadowEffect = null;

					_blurEffect?.Dispose();
					_blurEffect = null;
				}
				else
				{
					return _effectContext;
				}
			}

			_effectContext = new CanvasRenderTarget(_session, _canvasSize);
			return _effectContext;
		}

		private void DrawShadow(Action<CanvasDrawingSession> drawingAction)
		{
			var context = GetOrCreateEffectContext();
			if (context != null)
			{
				using (var imageSession = context.CreateDrawingSession())
				{
					imageSession.Clear(WColors.Transparent);
					imageSession.Transform = CurrentState.Matrix.Translate(CurrentState.ShadowOffset.X, CurrentState.ShadowOffset.Y);
					drawingAction(imageSession);
				}

				if (_shadowEffect == null)
					_shadowEffect = new ShadowEffect();

				_shadowEffect.Source = context;
				_shadowEffect.BlurAmount = CurrentState.ActualShadowBlur / 3;
				_shadowEffect.ShadowColor = CurrentState.ShadowColor;

				_session.Transform = Matrix3x2.Identity;
				_session.DrawImage(_shadowEffect, 0, 0);
				_session.Transform = CurrentState.Matrix;
			}
		}

		private void DrawBlurred(Action<CanvasDrawingSession> drawingAction)
		{
			var context = GetOrCreateEffectContext();
			if (context != null)
			{
				using (var imageSession = context.CreateDrawingSession())
				{
					imageSession.Clear(WColors.Transparent);
					imageSession.Transform = CurrentState.Matrix;
					drawingAction(imageSession);
				}

				if (_blurEffect == null)
					_blurEffect = new GaussianBlurEffect();

				_blurEffect.Source = context;
				_blurEffect.BorderMode = EffectBorderMode.Soft;
				_blurEffect.Optimization = EffectOptimization.Speed;

				_session.Transform = Matrix3x2.Identity;
				_session.DrawImage(_blurEffect, 0, 0);
				_session.Transform = CurrentState.Matrix;
			}
		}

		private void SetRect(float x, float y, float width, float height)
		{
			_rect.X = Math.Min(x, x + width);
			_rect.Y = Math.Min(y, y + height);
			_rect.Width = Math.Abs(width);
			_rect.Height = Math.Abs(height);
		}
	}
}
