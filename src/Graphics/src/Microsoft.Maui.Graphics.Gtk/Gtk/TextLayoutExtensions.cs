namespace Microsoft.Maui.Graphics.Platform.Gtk {

	internal static class TextLayoutExtensions {

		public static void SetFontStyle(this TextLayout it, IFont font, double? size = null, int? weight = null, FontStyleType? fontStyleType = null) {

			it.FontFamily = font.Name;

			if (weight.HasValue)
				it.Weight = weight.Value.ToPangoWeight();

			if (size.HasValue)
				it.PangoFontSize = size.Value.ScaledToPango();

			if (fontStyleType.HasValue)
				it.Style = fontStyleType.Value switch
				{
					FontStyleType.Normal => Pango.Style.Normal,
					FontStyleType.Italic => Pango.Style.Italic,
					FontStyleType.Oblique => Pango.Style.Oblique,
					_ => Pango.Style.Normal,
				};
		}

		public static void SetCanvasState(this TextLayout it, PlatformCanvasState state) {

			var font = (state?.Font ?? Font.Default).ToFontDescription();

			it.FontFamily = font.Family;
			it.Weight = font.Weight;

			it.PangoFontSize = state.FontSize.ScaledToPango();
			it.TextColor = state.FontColor;
		}

		public static TextLayout WithCanvasState(this TextLayout it, PlatformCanvasState state) {
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
