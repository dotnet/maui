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
	}
}
