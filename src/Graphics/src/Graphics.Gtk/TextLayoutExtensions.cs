namespace Microsoft.Maui.Graphics.Platform.Gtk;

internal static class TextLayoutExtensions {

	public static void SetFontStyle(this TextLayout it, IFont font, double? fontSize = null, int? weight = null, FontStyleType? fontStyleType = null) {

		if (font is { })
			it.FontFamily = font.Name;

		if (weight.HasValue)
			it.Weight = weight.Value.ToPangoWeight();

		if (fontSize.HasValue)
			it.PangoFontSize = fontSize.Value.ScaledToPango();

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

		it.PangoFontSize = state?.FontSize.ScaledToPango() ?? 10;
		it.TextColor = state?.FontColor ?? new Cairo.Color();
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

	public static Pango.WrapMode ToPangoWrap(this LineBreakMode it)
	{
		if (it.HasFlag(LineBreakMode.CharacterWrap))
			return Pango.WrapMode.Char;

		if (it.HasFlag(LineBreakMode.WordCharacterWrap))
			return Pango.WrapMode.WordChar;

		return Pango.WrapMode.Word;
	}

	public static Pango.EllipsizeMode ToPangoEllipsize(this LineBreakMode it) {

		if (it.HasFlag(LineBreakMode.Ellipsis | LineBreakMode.Tail))
			return Pango.EllipsizeMode.End;

		if (it.HasFlag(LineBreakMode.Ellipsis | LineBreakMode.Middle))
			return Pango.EllipsizeMode.Middle;

		if (it.HasFlag(LineBreakMode.Ellipsis | LineBreakMode.Head))
			return Pango.EllipsizeMode.Start;

		if (it.HasFlag(LineBreakMode.Ellipsis))
			return Pango.EllipsizeMode.End;

		return Pango.EllipsizeMode.None;
	}

}