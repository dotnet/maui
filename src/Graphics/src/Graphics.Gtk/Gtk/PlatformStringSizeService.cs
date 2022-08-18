namespace Microsoft.Maui.Graphics.Platform.Gtk;

public class PlatformStringSizeService : IStringSizeService {
	Cairo.Context? _sharedContext;

	internal Cairo.Context SharedContext {
		get {
			if (_sharedContext == null) {
				using var sf = new Cairo.ImageSurface (Cairo.Format.ARGB32, 1, 1);
				_sharedContext = new Cairo.Context (sf);
			}

			return _sharedContext;
		}
	}

	private static TextLayout? _textLayout;

	private TextLayout SharedTextLayout => _textLayout ??= new TextLayout (SharedContext) {
		HeightForWidth = true
	};

	public SizeF GetStringSize (string value, IFont font, float fontSize) {
		if (string.IsNullOrEmpty (value))
			return new SizeF ();

		lock (SharedTextLayout) {
			SharedTextLayout.SetFontStyle (font, fontSize);
			var size = SharedTextLayout.GetPixelSize (value);
			return new SizeF (size.width, size.height);
		}
	}

	public SizeF GetStringSize (string value, IFont font, float aFontSize, HorizontalAlignment aHorizontalAlignment, VerticalAlignment aVerticalAlignment) {
		if (string.IsNullOrEmpty (value))
			return new SizeF ();

		lock (SharedTextLayout) {
			SharedTextLayout.SetFontStyle (font, aFontSize);

			SharedTextLayout.HorizontalAlignment = aHorizontalAlignment;
			SharedTextLayout.VerticalAlignment = aVerticalAlignment;
			var size = SharedTextLayout.GetPixelSize (value);
			return new SizeF (size.width, size.height);
		}
	}
}