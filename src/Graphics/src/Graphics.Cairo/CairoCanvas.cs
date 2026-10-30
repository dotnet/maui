using System;
using System.IO;
using System.Numerics;
using Microsoft.Maui.Graphics.Text;

namespace Microsoft.Maui.Graphics.Cairo
{
	/// <summary>
	/// An <see cref="ICanvas"/> that draws with Cairo (shapes, paths, images, clipping, gradients, shadows) and
	/// Pango (text) into any <see cref="CairoContext"/>: an image surface, a GTK snapshot's Cairo node, a PDF/SVG surface...
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// <item>Angles follow the .NET MAUI Graphics convention (degrees, counter-clockwise from 3 o'clock).</item>
	/// <item>Dash patterns and dash offsets are expressed in multiples of the stroke size, like the other canvases.</item>
	/// <item>Shadow offset and blur, and <see cref="SetBlur(float)"/>, are in device pixels and are not affected by the current transform.</item>
	/// <item>The canvas does not own the Cairo context: dispose it yourself. Transform and clip are the context's own.</item>
	/// <item>The Cairo operators behind <see cref="BlendMode"/> require Cairo 1.10 or later; <see cref="BlendMode.PlusDarker"/> has no equivalent and draws normally.</item>
	/// </list>
	/// </remarks>
	public class CairoCanvas : AbstractCanvas<CairoCanvasState>, IBlurrableCanvas
	{
		private const int MaxLayerPixels = 64 * 1024 * 1024;

		private readonly CairoCanvasStateService _stateService;
		private CairoContext _context;

		/// <summary>
		/// Creates a canvas that is not attached to a context yet; set <see cref="Context"/> before drawing.
		/// </summary>
		public CairoCanvas() : this(null)
		{
		}

		/// <summary>
		/// Creates a canvas that draws into <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The Cairo context. It is not owned by the canvas.</param>
		public CairoCanvas(CairoContext context)
			: base(CreateStateService(out var stateService), new PangoStringSizeService())
		{
			_stateService = stateService;
			_context = context;
		}

		private static CairoCanvasStateService CreateStateService(out CairoCanvasStateService stateService) =>
			stateService = new CairoCanvasStateService();

		/// <summary>
		/// Gets or sets the Cairo context that is drawn into. Setting it resets the canvas state.
		/// </summary>
		public CairoContext Context
		{
			get => _context;
			set
			{
				_context = null;
				ResetState();
				_context = value;
			}
		}

		private CairoContext Ctx =>
			_context ?? throw new InvalidOperationException("The CairoCanvas has no Context. Set CairoCanvas.Context before drawing.");

		// ---------------------------------------------------------------- state

		/// <inheritdoc/>
		public override bool Antialias
		{
			set => CurrentState.Antialias = value;
		}

		/// <inheritdoc/>
		protected override float PlatformStrokeSize
		{
			set => CurrentState.PlatformStrokeSize = value;
		}

		/// <inheritdoc/>
		public override float MiterLimit
		{
			set => CurrentState.MiterLimit = value;
		}

		/// <inheritdoc/>
		public override float Alpha
		{
			set => CurrentState.Alpha = value;
		}

		/// <inheritdoc/>
		public override LineCap StrokeLineCap
		{
			set => CurrentState.StrokeLineCap = value;
		}

		/// <inheritdoc/>
		public override LineJoin StrokeLineJoin
		{
			set => CurrentState.StrokeLineJoin = value;
		}

		/// <inheritdoc/>
		public override Color StrokeColor
		{
			set => CurrentState.StrokeColor = value ?? Colors.Black;
		}

		/// <inheritdoc/>
		public override Color FontColor
		{
			set => CurrentState.FontColor = value ?? Colors.Black;
		}

		/// <inheritdoc/>
		public override IFont Font
		{
			set => CurrentState.Font = value;
		}

		/// <inheritdoc/>
		public override float FontSize
		{
			set => CurrentState.FontSize = value;
		}

		/// <inheritdoc/>
		public override Color FillColor
		{
			set
			{
				CurrentState.SetFillPattern(null);
				CurrentState.FillColor = value ?? Colors.White;
			}
		}

		/// <inheritdoc/>
		public override BlendMode BlendMode
		{
			set => CurrentState.BlendMode = value;
		}

		/// <summary>
		/// Sets a blur that is applied to everything drawn afterwards (until the state is restored).
		/// </summary>
		/// <param name="blurRadius">The blur radius in device pixels; 0 or less disables the blur.</param>
		public void SetBlur(float blurRadius) => CurrentState.BlurRadius = Math.Max(0, blurRadius);

		/// <inheritdoc/>
		protected override void PlatformSetStrokeDashPattern(float[] strokePattern, float strokeDashOffset, float strokeSize)
		{
			var state = CurrentState;
			if (strokePattern == null || strokePattern.Length == 0 || strokeSize <= 0)
			{
				state.PlatformDashPattern = null;
				state.PlatformDashOffset = 0;
				return;
			}

			var dashes = new double[strokePattern.Length];
			var sum = 0d;
			for (var i = 0; i < dashes.Length; i++)
			{
				dashes[i] = Math.Max(0, strokePattern[i]) * strokeSize;
				sum += dashes[i];
			}

			if (sum <= 0)
			{
				state.PlatformDashPattern = null;
				state.PlatformDashOffset = 0;
				return;
			}

			state.PlatformDashPattern = dashes;
			state.PlatformDashOffset = strokeDashOffset * strokeSize;
		}

		/// <inheritdoc/>
		public override void SaveState()
		{
			Ctx.Save();
			base.SaveState();
		}

		/// <inheritdoc/>
		protected override void StateRestored(CairoCanvasState state)
		{
			_context?.Restore();
		}

		/// <inheritdoc/>
		public override void ResetState()
		{
			base.ResetState();
			_stateService.Reset(CurrentState);
		}

		/// <inheritdoc/>
		public override void Dispose()
		{
			base.Dispose();
		}

		/// <inheritdoc/>
		/// <remarks>
		/// The shadow is a blurred silhouette of whatever is drawn next, offset in device pixels. A zero offset with no
		/// blur removes the shadow.
		/// </remarks>
		public override void SetShadow(SizeF offset, float blur, Color color)
		{
			var state = CurrentState;
			if (offset.Width == 0 && offset.Height == 0 && blur <= 0)
			{
				state.ClearShadow();
				return;
			}

			state.SetShadow(offset, blur, color ?? Colors.Black);
		}

		// ---------------------------------------------------------------- fill paint

		/// <inheritdoc/>
		public override void SetFillPaint(Paint paint, RectF rectangle)
		{
			var state = CurrentState;
			paint ??= Colors.White.AsPaint();

			if (paint is SolidPaint solidPaint)
			{
				FillColor = solidPaint.Color;
				return;
			}

			CairoPattern pattern = null;

			try
			{
				if (paint is LinearGradientPaint linearGradientPaint)
				{
					var x1 = linearGradientPaint.StartPoint.X * rectangle.Width + rectangle.X;
					var y1 = linearGradientPaint.StartPoint.Y * rectangle.Height + rectangle.Y;
					var x2 = linearGradientPaint.EndPoint.X * rectangle.Width + rectangle.X;
					var y2 = linearGradientPaint.EndPoint.Y * rectangle.Height + rectangle.Y;

					var gradient = new global::Cairo.LinearGradient(x1, y1, x2, y2);
					pattern = gradient;
					AddStops(gradient, linearGradientPaint.GetSortedStops());
				}
				else if (paint is RadialGradientPaint radialGradientPaint)
				{
					var centerX = radialGradientPaint.Center.X * rectangle.Width + rectangle.X;
					var centerY = radialGradientPaint.Center.Y * rectangle.Height + rectangle.Y;
					var radius = radialGradientPaint.Radius * Math.Max(rectangle.Height, rectangle.Width);
					if (radius <= 0)
						radius = GeometryUtil.GetDistance(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);

					var gradient = new global::Cairo.RadialGradient(centerX, centerY, 0, centerX, centerY, radius);
					pattern = gradient;
					AddStops(gradient, radialGradientPaint.GetSortedStops());
				}
				else if (paint is PatternPaint patternPaint)
				{
					pattern = CreatePatternPaintPattern(patternPaint);
				}
				else if (paint is ImagePaint imagePaint)
				{
					var image = ResolveImage(imagePaint.Image, out var temporary);
					if (image?.PlatformRepresentation != null)
					{
						// Cairo keeps its own reference to the surface, so the image may be disposed afterwards.
						pattern = new CairoSurfacePattern(image.PlatformRepresentation)
						{
							Extend = global::Cairo.Extend.Repeat,
							Filter = global::Cairo.Filter.Good,
						};
					}

					if (temporary)
						image?.Dispose();
				}
			}
			catch (Exception exception)
			{
				System.Diagnostics.Debug.WriteLine(exception);
				pattern?.Dispose();
				pattern = null;
			}

			if (pattern == null)
			{
				FillColor = paint is GradientPaint gradientPaint ? gradientPaint.BlendStartAndEndColors() : paint.BackgroundColor ?? Colors.White;
				return;
			}

			state.SetFillPattern(pattern);
		}

		private static void AddStops(global::Cairo.Gradient gradient, PaintGradientStop[] stops)
		{
			foreach (var stop in stops)
			{
				var color = stop.Color ?? Colors.White;
				gradient.AddColorStop(stop.Offset, new global::Cairo.Color(color.Red, color.Green, color.Blue, color.Alpha));
			}
		}

		private CairoPattern CreatePatternPaintPattern(PatternPaint patternPaint)
		{
			var pattern = patternPaint.Pattern;
			if (pattern == null)
				return null;

			var scale = DisplayScale > 0 ? DisplayScale : 1;
			var width = Math.Max(1, (int)Math.Ceiling(pattern.Width * scale));
			var height = Math.Max(1, (int)Math.Ceiling(pattern.Height * scale));

			var surface = new CairoImageSurface(CairoFormat.Argb32, width, height);
			try
			{
				using (var context = new CairoContext(surface))
				using (var canvas = new CairoCanvas(context))
				{
					canvas.Scale(scale, scale);
					pattern.Draw(canvas);
				}

				surface.Flush();
				var result = new CairoSurfacePattern(surface)
				{
					Extend = global::Cairo.Extend.Repeat,
					Filter = global::Cairo.Filter.Good,
				};

				// The tile is rendered at device resolution; the pattern space is user space.
				var matrix = new CairoMatrix();
				matrix.InitScale(scale, scale);
				result.Matrix = matrix;
				return result;
			}
			finally
			{
				// The pattern holds its own reference to the surface.
				surface.Dispose();
			}
		}

		// ---------------------------------------------------------------- rendering core

		private readonly struct Bounds
		{
			public Bounds(double x, double y, double width, double height)
			{
				X = x;
				Y = y;
				Width = width;
				Height = height;
				IsKnown = true;
			}

			public double X { get; }
			public double Y { get; }
			public double Width { get; }
			public double Height { get; }
			public bool IsKnown { get; }

			public static Bounds Unknown => default;
		}

		private static Bounds BoundsOf(float x, float y, float width, float height)
		{
			var left = Math.Min(x, x + width);
			var top = Math.Min(y, y + height);
			return new Bounds(left, top, Math.Abs(width), Math.Abs(height));
		}

		private static Bounds Inflate(Bounds bounds, double amount) =>
			bounds.IsKnown ? new Bounds(bounds.X - amount, bounds.Y - amount, bounds.Width + 2 * amount, bounds.Height + 2 * amount) : bounds;

		private double StrokeInflate() =>
			CurrentState.PlatformStrokeSize * Math.Max(CurrentState.MiterLimit, 1.5f) / 2 + 1;

		private static void SetSolid(CairoContext context, Color color, float alpha)
		{
			var c = color ?? Colors.Black;
			context.SetSourceRGBA(c.Red, c.Green, c.Blue, c.Alpha * alpha);
		}

		private void SetStrokeStyle(CairoContext context)
		{
			var state = CurrentState;
			context.LineWidth = state.PlatformStrokeSize;
			context.LineCap = state.StrokeLineCap switch
			{
				LineCap.Round => global::Cairo.LineCap.Round,
				LineCap.Square => global::Cairo.LineCap.Square,
				_ => global::Cairo.LineCap.Butt,
			};
			context.LineJoin = state.StrokeLineJoin switch
			{
				LineJoin.Round => global::Cairo.LineJoin.Round,
				LineJoin.Bevel => global::Cairo.LineJoin.Bevel,
				_ => global::Cairo.LineJoin.Miter,
			};
			context.MiterLimit = Math.Max(1, state.MiterLimit);

			var dashes = state.PlatformDashPattern;
			context.SetDash(dashes ?? Array.Empty<double>(), dashes == null ? 0 : state.PlatformDashOffset);
		}

		/// <summary>
		/// Runs <paramref name="draw"/> against the context, adding the shadow and blur layers when they are active.
		/// The delegate receives the context to draw into and whether it draws the shadow silhouette (it must then use
		/// <see cref="SetShadowSource"/> instead of the normal paint).
		/// </summary>
		private void Render(Bounds bounds, Action<CairoContext, bool> draw)
		{
			var context = Ctx;
			var state = CurrentState;

			context.Operator = state.BlendMode.AsCairoOperator();
			context.Antialias = state.Antialias ? global::Cairo.Antialias.Default : global::Cairo.Antialias.None;

			if (state.HasShadow)
				RenderLayer(context, bounds, draw, true);

			if (state.BlurRadius > 0)
				RenderLayer(context, bounds, draw, false);
			else
				draw(context, false);
		}

		private void SetShadowSource(CairoContext context)
		{
			var state = CurrentState;
			SetSolid(context, state.ShadowColor, state.Alpha);
		}

		private void RenderLayer(CairoContext context, Bounds bounds, Action<CairoContext, bool> draw, bool shadow)
		{
			var state = CurrentState;
			var sigma = (shadow ? state.ShadowBlur : state.BlurRadius) / 2d;
			var offsetX = shadow ? state.ShadowOffset.Width : 0d;
			var offsetY = shadow ? state.ShadowOffset.Height : 0d;
			var padding = Math.Ceiling(sigma * 3) + 2;

			// Device-space box of the area to render: the shape, limited to what the clip can show.
			double minX, minY, maxX, maxY;
			var clip = context.ClipExtents();
			DeviceBox(context, clip.X, clip.Y, clip.Width, clip.Height, out var clipMinX, out var clipMinY, out var clipMaxX, out var clipMaxY);
			// The layer is painted at +offset, so the part of it that can be visible is the clip box shifted by -offset.
			clipMinX = clipMinX - offsetX - padding;
			clipMaxX = clipMaxX - offsetX + padding;
			clipMinY = clipMinY - offsetY - padding;
			clipMaxY = clipMaxY - offsetY + padding;

			if (bounds.IsKnown)
			{
				DeviceBox(context, bounds.X, bounds.Y, bounds.Width, bounds.Height, out minX, out minY, out maxX, out maxY);
				minX = Math.Max(minX - padding, clipMinX);
				minY = Math.Max(minY - padding, clipMinY);
				maxX = Math.Min(maxX + padding, clipMaxX);
				maxY = Math.Min(maxY + padding, clipMaxY);
			}
			else
			{
				minX = clipMinX;
				minY = clipMinY;
				maxX = clipMaxX;
				maxY = clipMaxY;
			}

			if (!(maxX > minX) || !(maxY > minY))
				return;

			var x0 = (int)Math.Floor(minX);
			var y0 = (int)Math.Floor(minY);
			var width = (int)Math.Ceiling(maxX) - x0;
			var height = (int)Math.Ceiling(maxY) - y0;
			if (width <= 0 || height <= 0 || (long)width * height > MaxLayerPixels)
			{
				if (!shadow)
					draw(context, false);
				return;
			}

			using (var layer = new CairoImageSurface(CairoFormat.Argb32, width, height))
			{
				using (var layerContext = new CairoContext(layer))
				{
					var matrix = context.Matrix;
					matrix.X0 -= x0;
					matrix.Y0 -= y0;
					layerContext.Matrix = matrix;
					layerContext.Antialias = context.Antialias;
					draw(layerContext, shadow);
				}

				layer.Flush();
				SurfaceBlur.Apply(layer, sigma);

				context.Save();
				context.IdentityMatrix();
				context.SetSource(layer, x0 + offsetX, y0 + offsetY);
				context.Paint();
				context.Restore();
			}
		}

		private static void DeviceBox(CairoContext context, double x, double y, double width, double height, out double minX, out double minY, out double maxX, out double maxY)
		{
			double ax = x, ay = y;
			double bx = x + width, by = y;
			double cx = x + width, cy = y + height;
			double dx = x, dy = y + height;
			context.UserToDevice(ref ax, ref ay);
			context.UserToDevice(ref bx, ref by);
			context.UserToDevice(ref cx, ref cy);
			context.UserToDevice(ref dx, ref dy);
			minX = Math.Min(Math.Min(ax, bx), Math.Min(cx, dx));
			maxX = Math.Max(Math.Max(ax, bx), Math.Max(cx, dx));
			minY = Math.Min(Math.Min(ay, by), Math.Min(cy, dy));
			maxY = Math.Max(Math.Max(ay, by), Math.Max(cy, dy));
		}

		private void StrokeShape(Bounds bounds, Action<CairoContext> addPath)
		{
			var state = CurrentState;
			if (state.PlatformStrokeSize <= 0)
				return;

			Render(Inflate(bounds, StrokeInflate()), (context, shadow) =>
			{
				context.NewPath();
				addPath(context);
				SetStrokeStyle(context);
				if (shadow)
					SetShadowSource(context);
				else
					SetSolid(context, state.StrokeColor, state.Alpha);
				context.Stroke();
			});
		}

		private void FillShape(Bounds bounds, WindingMode windingMode, Action<CairoContext> addPath)
		{
			var state = CurrentState;

			Render(bounds, (context, shadow) =>
			{
				context.NewPath();
				addPath(context);
				context.FillRule = windingMode == WindingMode.EvenOdd ? global::Cairo.FillRule.EvenOdd : global::Cairo.FillRule.Winding;

				if (shadow)
				{
					SetShadowSource(context);
					context.Fill();
				}
				else if (state.FillPattern != null)
				{
					// Fill patterns take the global alpha at paint time.
					context.Save();
					context.Clip();
					context.SetSource(state.FillPattern);
					context.PaintWithAlpha(state.Alpha);
					context.Restore();
				}
				else
				{
					SetSolid(context, state.FillColor, state.Alpha);
					context.Fill();
				}
			});
		}

		// ---------------------------------------------------------------- shapes

		/// <inheritdoc/>
		protected override void PlatformDrawLine(float x1, float y1, float x2, float y2) =>
			StrokeShape(new Bounds(Math.Min(x1, x2), Math.Min(y1, y2), Math.Abs(x2 - x1), Math.Abs(y2 - y1)), context =>
			{
				context.MoveTo(x1, y1);
				context.LineTo(x2, y2);
			});

		/// <inheritdoc/>
		protected override void PlatformDrawArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise, bool closed) =>
			StrokeShape(BoundsOf(x, y, width, height), context =>
			{
				context.AddArc(x, y, width, height, startAngle, endAngle, clockwise, true);
				if (closed)
					context.ClosePath();
			});

		/// <inheritdoc/>
		public override void FillArc(float x, float y, float width, float height, float startAngle, float endAngle, bool clockwise) =>
			FillShape(BoundsOf(x, y, width, height), WindingMode.NonZero, context =>
			{
				context.AddArc(x, y, width, height, startAngle, endAngle, clockwise, true);
				context.ClosePath();
			});

		/// <inheritdoc/>
		protected override void PlatformDrawRectangle(float x, float y, float width, float height) =>
			StrokeShape(BoundsOf(x, y, width, height), context => context.Rectangle(x, y, width, height));

		/// <inheritdoc/>
		public override void FillRectangle(float x, float y, float width, float height) =>
			FillShape(BoundsOf(x, y, width, height), WindingMode.NonZero, context => context.Rectangle(x, y, width, height));

		/// <inheritdoc/>
		protected override void PlatformDrawRoundedRectangle(float x, float y, float width, float height, float cornerRadius) =>
			StrokeShape(BoundsOf(x, y, width, height), context => context.AddRoundedRectangle(x, y, width, height, cornerRadius));

		/// <inheritdoc/>
		public override void FillRoundedRectangle(float x, float y, float width, float height, float cornerRadius) =>
			FillShape(BoundsOf(x, y, width, height), WindingMode.NonZero, context => context.AddRoundedRectangle(x, y, width, height, cornerRadius));

		/// <inheritdoc/>
		protected override void PlatformDrawEllipse(float x, float y, float width, float height) =>
			StrokeShape(BoundsOf(x, y, width, height), context => context.AddEllipse(x, y, width, height));

		/// <inheritdoc/>
		public override void FillEllipse(float x, float y, float width, float height) =>
			FillShape(BoundsOf(x, y, width, height), WindingMode.NonZero, context => context.AddEllipse(x, y, width, height));

		/// <inheritdoc/>
		protected override void PlatformDrawPath(PathF path)
		{
			if (path == null)
				return;

			var b = path.Bounds;
			StrokeShape(new Bounds(b.X, b.Y, b.Width, b.Height), context => context.AddPath(path));
		}

		/// <inheritdoc/>
		public override void FillPath(PathF path, WindingMode windingMode)
		{
			if (path == null)
				return;

			var b = path.Bounds;
			FillShape(new Bounds(b.X, b.Y, b.Width, b.Height), windingMode, context => context.AddPath(path));
		}

		// ---------------------------------------------------------------- clip

		/// <inheritdoc/>
		public override void ClipRectangle(float x, float y, float width, float height)
		{
			var context = Ctx;
			context.NewPath();
			context.Rectangle(x, y, width, height);
			context.Clip();
		}

		/// <inheritdoc/>
		public override void ClipPath(PathF path, WindingMode windingMode = WindingMode.NonZero)
		{
			if (path == null)
				return;

			var context = Ctx;
			var previous = context.FillRule;
			context.NewPath();
			context.AddPath(path);
			context.FillRule = windingMode == WindingMode.EvenOdd ? global::Cairo.FillRule.EvenOdd : global::Cairo.FillRule.Winding;
			context.Clip();
			context.FillRule = previous;
		}

		/// <inheritdoc/>
		/// <remarks>
		/// Cairo has no "difference" clip: the clip becomes (current clip box + rectangle) filled with the even-odd rule,
		/// i.e. the part of the current clip that is outside the rectangle.
		/// </remarks>
		public override void SubtractFromClip(float x, float y, float width, float height)
		{
			var context = Ctx;
			var extents = context.ClipExtents();
			var previous = context.FillRule;

			context.NewPath();
			context.Rectangle(extents.X - 1, extents.Y - 1, extents.Width + 2, extents.Height + 2);
			context.Rectangle(x, y, width, height);
			context.FillRule = global::Cairo.FillRule.EvenOdd;
			context.Clip();
			context.FillRule = previous;
		}

		// ---------------------------------------------------------------- text

		/// <inheritdoc/>
		public override void DrawString(string value, float x, float y, HorizontalAlignment horizontalAlignment)
		{
			if (string.IsNullOrEmpty(value))
				return;

			var state = CurrentState;
			var context = Ctx;
			using var layout = PangoText.CreateLayout(context, value, state.Font, state.FontSize);
			PangoText.Measure(layout, out var textWidth, out var textHeight, out var baseline);

			var left = horizontalAlignment switch
			{
				HorizontalAlignment.Right => x - textWidth,
				HorizontalAlignment.Center => x - textWidth / 2,
				_ => (double)x,
			};
			var top = y - baseline;

			DrawLayout(layout, left, top, new Bounds(left, top, textWidth, textHeight), false, default);
		}

		/// <inheritdoc/>
		public override void DrawString(string value, float x, float y, float width, float height, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, TextFlow textFlow = TextFlow.ClipBounds, float lineSpacingAdjustment = 0)
		{
			if (string.IsNullOrEmpty(value))
				return;

			var state = CurrentState;
			var layout = PangoText.CreateLayout(Ctx, value, state.Font, state.FontSize);
			try
			{
				DrawBoxedLayout(layout, x, y, width, height, horizontalAlignment, verticalAlignment, textFlow, lineSpacingAdjustment);
			}
			finally
			{
				layout.Dispose();
			}
		}

		/// <inheritdoc/>
		/// <remarks>Supported run attributes: font name and size, bold, italic, underline, strikethrough, sub/superscript, color and background.</remarks>
		public override void DrawText(IAttributedText value, float x, float y, float width, float height)
		{
			if (value == null || string.IsNullOrEmpty(value.Text))
				return;

			var state = CurrentState;
			var layout = PangoText.CreateLayout(Ctx, string.Empty, state.Font, state.FontSize);
			try
			{
				layout.SetMarkup(PangoText.ToMarkup(value));
				DrawBoxedLayout(layout, x, y, width, height, HorizontalAlignment.Left, VerticalAlignment.Top, TextFlow.ClipBounds, 0);
			}
			finally
			{
				layout.Dispose();
			}
		}

		private void DrawBoxedLayout(Pango.Layout layout, float x, float y, float width, float height, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, TextFlow textFlow, float lineSpacingAdjustment)
		{
			if (width > 0)
			{
				layout.Width = (int)Math.Min(int.MaxValue, width * PangoText.PangoScale);
				layout.Wrap = Pango.WrapMode.WordChar;
			}

			layout.Alignment = horizontalAlignment switch
			{
				HorizontalAlignment.Center => Pango.Alignment.Center,
				HorizontalAlignment.Right => Pango.Alignment.Right,
				_ => Pango.Alignment.Left,
			};
			layout.Justify = horizontalAlignment == HorizontalAlignment.Justified;

			if (lineSpacingAdjustment != 0)
				layout.Spacing = (int)(lineSpacingAdjustment * PangoText.PangoScale);

			PangoText.Measure(layout, out var textWidth, out var textHeight, out _);

			double top = y;
			if (verticalAlignment == VerticalAlignment.Center)
				top = y + (height - textHeight) / 2;
			else if (verticalAlignment == VerticalAlignment.Bottom)
				top = y + height - textHeight;

			var clip = textFlow == TextFlow.ClipBounds;
			var textBounds = new Bounds(Math.Min(x, x + (float)textWidth), top, Math.Max(width, (float)textWidth), textHeight);
			DrawLayout(layout, x, top, textBounds, clip, new Bounds(x, y, width, height));
		}

		private void DrawLayout(Pango.Layout layout, double left, double top, Bounds bounds, bool clip, Bounds clipBounds)
		{
			var state = CurrentState;

			Render(Inflate(bounds, 2), (context, shadow) =>
			{
				context.Save();
				context.NewPath();
				if (clip)
				{
					context.Rectangle(clipBounds.X, clipBounds.Y, clipBounds.Width, clipBounds.Height);
					context.Clip();
				}

				if (shadow)
					SetShadowSource(context);
				else
					SetSolid(context, state.FontColor, state.Alpha);

				context.MoveTo(left, top);
				PangoText.ApplyTextOptions(context, state.Antialias);
				Pango.CairoHelper.UpdateLayout(context, layout);
				Pango.CairoHelper.ShowLayout(context, layout);
				context.Restore();
			});
		}

		// ---------------------------------------------------------------- images

		/// <inheritdoc/>
		public override void DrawImage(IImage image, float x, float y, float width, float height)
		{
			var cairoImage = ResolveImage(image, out var temporary);
			try
			{
				var surface = cairoImage?.PlatformRepresentation;
				if (surface == null || width == 0 || height == 0)
					return;

				var state = CurrentState;
				var left = Math.Min(x, x + width);
				var top = Math.Min(y, y + height);
				var w = Math.Abs(width);
				var h = Math.Abs(height);
				var imageWidth = surface.Width;
				var imageHeight = surface.Height;
				if (imageWidth <= 0 || imageHeight <= 0)
					return;

				Render(new Bounds(left, top, w, h), (context, shadow) =>
				{
					context.Save();
					context.NewPath();
					context.Translate(left, top);
					context.Scale(w / imageWidth, h / imageHeight);

					using (var source = new CairoSurfacePattern(surface))
					{
						source.Extend = global::Cairo.Extend.Pad;
						source.Filter = global::Cairo.Filter.Good;

						context.Rectangle(0, 0, imageWidth, imageHeight);
						context.Clip();

						if (shadow)
						{
							SetShadowSource(context);
							context.Mask(source);
						}
						else
						{
							context.SetSource(source);
							context.PaintWithAlpha(state.Alpha);
						}
					}

					context.Restore();
				});
			}
			finally
			{
				if (temporary)
					cairoImage.Dispose();
			}
		}

		// Images from another canvas implementation are re-encoded once so that they can be drawn.
		private static CairoImage ResolveImage(IImage image, out bool temporary)
		{
			temporary = false;
			if (image is CairoImage cairoImage)
				return cairoImage;

			if (image == null)
				return null;

			try
			{
				using (var stream = new MemoryStream())
				{
					image.Save(stream, ImageFormat.Png);
					var bytes = stream.ToArray();
					var converted = CairoImage.FromBytes(bytes);
					temporary = converted != null;
					return converted;
				}
			}
			catch (Exception exception)
			{
				System.Diagnostics.Debug.WriteLine(exception);
				return null;
			}
		}

		// ---------------------------------------------------------------- transforms

		/// <inheritdoc/>
		protected override void PlatformRotate(float degrees, float radians, float x, float y)
		{
			var context = Ctx;
			context.Translate(x, y);
			context.Rotate(radians);
			context.Translate(-x, -y);
		}

		/// <inheritdoc/>
		protected override void PlatformRotate(float degrees, float radians) => Ctx.Rotate(radians);

		/// <inheritdoc/>
		protected override void PlatformScale(float fx, float fy) => Ctx.Scale(fx, fy);

		/// <inheritdoc/>
		protected override void PlatformTranslate(float tx, float ty) => Ctx.Translate(tx, ty);

		/// <inheritdoc/>
		protected override void PlatformConcatenateTransform(Matrix3x2 transform) => Ctx.Transform(transform.AsCairoMatrix());
	}
}
