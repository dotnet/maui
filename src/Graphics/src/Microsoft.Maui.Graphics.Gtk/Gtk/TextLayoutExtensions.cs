namespace Microsoft.Maui.Graphics.Native.Gtk {

	public static class TextLayoutExtensions {

		public static void SetFontStyle(this TextLayout it, IFontStyle fs) {
			it.FontFamily = fs.FontFamily.Name;
			it.Weight = FontExtensions.ToFontWeigth(fs.Weight);
			it.Style = fs.StyleType.ToPangoStyle();

			if (fs is NativeFontStyle nfs) {
				it.PangoFontSize = nfs.Size.ScaledToPango();
			}
		}

		public static void SetCanvasState(this TextLayout it, NativeCanvasState state) {
			it.FontFamily = state.FontName;
			it.PangoFontSize = state.FontSize.ScaledToPango();
			it.TextColor = state.FontColor;
		}

		public static TextLayout WithCanvasState(this TextLayout it, NativeCanvasState state) {
			it.SetCanvasState(state);

			return it;
		}

		public static Size GetSize(this TextLayout it, string text, float textHeigth) {
			var (width, height) = it.GetPixelSize(text, (int) textHeigth);

			return new Size(width, height);
		}

		public static Pango.Alignment ToPango(this HorizontalAlignment it) => it switch {
			HorizontalAlignment.Center => Pango.Alignment.Center,
			HorizontalAlignment.Right => Pango.Alignment.Right,
			_ => Pango.Alignment.Left
		};

		public static Pango.WrapMode ToPangoWrap(this Extras.LineBreakMode it) {
			if (it.HasFlag(Extras.LineBreakMode.CharacterWrap))
				return Pango.WrapMode.Char;
			else if (it.HasFlag(Extras.LineBreakMode.WordCharacterWrap))
				return Pango.WrapMode.WordChar;
			else
				return Pango.WrapMode.Word;
		}

		public static Pango.EllipsizeMode ToPangoEllipsize(this Extras.LineBreakMode it) {

			if (it.HasFlag(Extras.LineBreakMode.Elipsis | Extras.LineBreakMode.End))
				return Pango.EllipsizeMode.End;

			if (it.HasFlag(Extras.LineBreakMode.Elipsis | Extras.LineBreakMode.Center))
				return Pango.EllipsizeMode.Middle;

			if (it.HasFlag(Extras.LineBreakMode.Elipsis | Extras.LineBreakMode.Start))
				return Pango.EllipsizeMode.Start;

			if (it.HasFlag(Extras.LineBreakMode.Elipsis))
				return Pango.EllipsizeMode.End;

			return Pango.EllipsizeMode.None;
		}

	}

}
