using System;
using Microsoft.Maui.Graphics.Extras;
using Microsoft.Maui.Graphics.Text;
using Context = Cairo.Context;

namespace Microsoft.Maui.Graphics.Native.Gtk {

	/// <summary>
	/// Measures and draws text using <see cref="Pango.Layout"/>
	/// https://developer.gnome.org/pango/1.46/pango-Layout-Objects.html
	/// https://developer.gnome.org/gdk3/stable/gdk3-Pango-Interaction.html
	/// </summary>
	public class TextLayout : IDisposable {

		private Context _context;

		public TextLayout(Context context) {
			_context = context;
		}

		public Context Context => _context;

		public string FontFamily { get; set; }

		public Pango.Weight Weight { get; set; } = Pango.Weight.Normal;

		public Pango.Style Style { get; set; } = Pango.Style.Normal;

		public int PangoFontSize { get; set; } = -1;

		private Pango.Layout? _layout;
		private bool _layoutOwned = false;

		public TextFlow TextFlow { get; set; } = TextFlow.OverflowBounds;

		public HorizontalAlignment HorizontalAlignment { get; set; }

		public VerticalAlignment VerticalAlignment { get; set; }

		public LineBreakMode LineBreakMode { get; set; } = LineBreakMode.EndTruncation;

		public Cairo.Color TextColor { get; set; }

		public float LineSpacingAdjustment { get; set; }

		public bool HeightForWidth { get; set; } = true;

		public Action<TextLayout> BeforeDrawn { get; set; }

		public Action<TextLayout> AfterDrawn { get; set; }

		public void SetLayout(Pango.Layout value) {
			_layout = value;
			_layoutOwned = false;
		}

		private Pango.FontDescription? _fontDescription;
		private bool _fontDescriptionOwned = false;

		public Pango.FontDescription FontDescription {
			get {
				if (PangoFontSize == -1) {
					PangoFontSize = NativeFontService.Instance.SystemFontDescription.Size;
				}

				if (string.IsNullOrEmpty(FontFamily)) {
					FontFamily = NativeFontService.Instance.SystemFontDescription.Family;
				}

				if (_fontDescription == null) {
					_fontDescription = new Pango.FontDescription {
						Family = FontFamily,
						Weight = Weight,
						Style = Style,
						Size = PangoFontSize
					};

					_fontDescriptionOwned = true;

				}

				return _fontDescription;
			}
			set {
				_fontDescription = value;
				_fontDescriptionOwned = false;
			}
		}

		public Pango.Layout GetLayout() {
			if (_layout == null) {
				_layout = Pango.CairoHelper.CreateLayout(Context);
				_layoutOwned = true;
			}

			if (_layout.FontDescription != FontDescription) {
				_layout.FontDescription = FontDescription;
			}

			// allign & justify per Size
			_layout.Alignment = HorizontalAlignment.ToPango();
			_layout.Justify = HorizontalAlignment.HasFlag(HorizontalAlignment.Justified);
			_layout.Wrap = LineBreakMode.ToPangoWrap();
			_layout.Ellipsize = LineBreakMode.ToPangoEllipsize();

			// _layout.SingleParagraphMode = true;
			return _layout;
		}

		public void Dispose() {
			if (_fontDescriptionOwned) {
				_fontDescription?.Dispose();
			}

			if (_layoutOwned) {
				_layout?.Dispose();
			}
		}

		public (int width, int height) GetPixelSize(string text, double desiredSize = -1d) {

			var layout = GetLayout();

			if (desiredSize > 0) {
				if (HeightForWidth) {
					layout.Width = desiredSize.ScaledToPango();
				} else {
					layout.Height = desiredSize.ScaledToPango();
				}
			}

			layout.SetText(text);
			layout.GetPixelSize(out var textWidth, out var textHeight);

			return (textWidth, textHeight);
		}

		private void Draw() {
			if (_layout == null)
				return;

			Context.SetSourceRGBA(TextColor.R, TextColor.G, TextColor.B, TextColor.A);

			BeforeDrawn?.Invoke(this);

			// https://developer.gnome.org/pango/1.46/pango-Cairo-Rendering.html#pango-cairo-show-layout
			// Draws a PangoLayout in the specified cairo context.
			// The top-left corner of the PangoLayout will be drawn at the current point of the cairo context.
			Pango.CairoHelper.ShowLayout(Context, _layout);
		}

		private float GetX(float x, int width) => HorizontalAlignment switch {
			HorizontalAlignment.Left => x,
			HorizontalAlignment.Right => x - width,
			HorizontalAlignment.Center => x - width / 2f,
			_ => x
		};

		private float GetY(float y, int height) => VerticalAlignment switch {
			VerticalAlignment.Top => y,
			VerticalAlignment.Center => y - height,
			VerticalAlignment.Bottom => y - height / 2f,
			_ => y
		};

		private float GetDx(int width) => HorizontalAlignment switch {
			HorizontalAlignment.Left => 0,
			HorizontalAlignment.Center => width / 2f,
			HorizontalAlignment.Right => width,
			_ => 0
		};

		private float GetDy(int height) => VerticalAlignment switch {
			VerticalAlignment.Top => 0,
			VerticalAlignment.Center => height / 2f,
			VerticalAlignment.Bottom => height,
			_ => 0
		};

		public void DrawString(string value, float x, float y) {

			Context.Save();

			var layout = GetLayout();
			layout.SetText(value);
			layout.GetPixelSize(out var textWidth, out var textHeight);

			if (layout.IsWrapped || layout.IsEllipsized) {
				if (HeightForWidth)
					layout.Width = textWidth.ScaledToPango();
				else
					layout.Height = textWidth.ScaledToPango();
			}

			var mX = GetX(x, textWidth);
			var mY = GetY(y, textHeight);
			Context.MoveTo(mX, mY);
			Draw();
			Context.Restore();

		}

		public void DrawString(string value, float x, float y, float width, float height) {

			Context.Save();
			Context.Translate(x, y);

			var layout = GetLayout();
			layout.SetText(value);

			if (HeightForWidth) {
				layout.Width = width.ScaledToPango();

				if (TextFlow == TextFlow.ClipBounds) {
					layout.Height = height.ScaledToPango();
				}
			} else {
				layout.Height = height.ScaledToPango();

				if (TextFlow == TextFlow.ClipBounds) {
					layout.Width = width.ScaledToPango();
				}
			}

			if (TextFlow == TextFlow.ClipBounds && !layout.IsEllipsized) {
				layout.Ellipsize = Pango.EllipsizeMode.End;
			}

			if (!layout.IsWrapped || !layout.IsEllipsized) {
				layout.Wrap = Pango.WrapMode.Char;
			}

			layout.GetPixelExtents(out var inkRect, out var logicalRect);

			var mX = HeightForWidth ?
				0 :
				TextFlow == TextFlow.ClipBounds ?
					Math.Max(0, GetDx((int) width - logicalRect.Width - logicalRect.X)) :
					GetDx((int) width - logicalRect.Width - inkRect.X);

			var mY = !HeightForWidth ?
				0 :
				TextFlow == TextFlow.ClipBounds ?
					Math.Max(0, GetDy((int) height - inkRect.Height - inkRect.Y)) :
					GetDy((int) height - inkRect.Height - inkRect.Y);

			if (mY + inkRect.Height > height && TextFlow == TextFlow.ClipBounds && !HeightForWidth) {
				mY = 0;
			}

			Context.MoveTo(mX, mY);
			Draw();
			Context.Restore();
		}

		[GtkMissingImplementation]
		public void DrawAttributedText(IAttributedText value, float f, float f1, float width, float height) { }

		#region future use for better TextFlow.ClipBounds - algo without Elipsize

		/// <summary>
		/// future use for
		/// </summary>
		private void ClampToContext() {
			if (_layout == null || _context == null)
				return;

			var ctxSize = Context.ClipExtents();

			_layout.GetExtents(out var inkRect, out var logicalRect);
			var maxW = ctxSize.Width.ScaledToPango();
			var maxH = ctxSize.Height.ScaledToPango();

			while (logicalRect.Width > maxW) {
				if (!_layout.IsWrapped) {
					_layout.Wrap = Pango.WrapMode.Char;
				}

				_layout.Width = maxW;
				_layout.GetExtents(out inkRect, out logicalRect);
				maxW -= 1.ScaledToPango();
			}

			while (logicalRect.Height > maxH) {
				if (!_layout.IsWrapped) {
					_layout.Wrap = Pango.WrapMode.Char;
				}

				_layout.Height = maxH;
				_layout.GetExtents(out inkRect, out logicalRect);
				maxH -= 1.ScaledToPango();
			}

			var resLr = new Size(logicalRect.Width.ScaledFromPango(), logicalRect.Height.ScaledFromPango());

		}

		/// <summary>
		/// Get the distance in pixels between the top of the layout bounds and the first line's baseline
		/// </summary>
		public double GetBaseline() {
			// Just get the first line
			using var iter = GetLayout().Iter;

			return Pango.Units.ToPixels(iter.Baseline);
		}

		/// <summary>
		/// Get the distance in pixels between the top of the layout bounds and the first line's meanline (usually equivalent to the baseline minus half of the x-height)
		/// </summary>
		public double GetMeanline() {
			var baseline = 0;

			var layout = GetLayout();
			using var iter = layout.Iter;

			baseline = iter.Baseline;

			var font = layout.Context.LoadFont(layout.FontDescription);

			return Pango.Units.ToPixels(baseline - font.GetMetrics(Pango.Language.Default).StrikethroughPosition);
		}

		#endregion

	}

}
