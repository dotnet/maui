namespace Xamarin.Forms.Platform.Tizen
{
	public static class TextAlignmentExtensions
	{
		public static Native.TextAlignment ToNative(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return Native.TextAlignment.Center;

				case TextAlignment.Start:
					return Native.TextAlignment.Start;

				case TextAlignment.End:
					return Native.TextAlignment.End;

				default:
					Log.Warn("Warning: unrecognized HorizontalTextAlignment value {0}. " +
						"Expected: {Start|Center|End}.", alignment);
					Log.Debug("Falling back to platform's default settings.");
					return Native.TextAlignment.Auto;
			}
		}

		public static double ToNativeDouble(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return 0.5d;

				case TextAlignment.Start:
					return 0;

				case TextAlignment.End:
					return 1d;

				default:
					Log.Warn("Warning: unrecognized HorizontalTextAlignment value {0}. " +
						"Expected: {Start|Center|End}.", alignment);
					Log.Debug("Falling back to platform's default settings.");
					return 0.5d;
			}
		}
	}
}
