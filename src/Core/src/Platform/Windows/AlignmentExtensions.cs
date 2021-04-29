namespace Microsoft.Maui
{
	public static class AlignmentExtensions
	{
		public static UI.Xaml.TextAlignment ToNative(this TextAlignment alignment, bool isLtr)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return UI.Xaml.TextAlignment.Center;
				case TextAlignment.End:
					if (isLtr)
						return UI.Xaml.TextAlignment.Right;
					else
						return UI.Xaml.TextAlignment.Left;
				default:
					if (isLtr)
						return UI.Xaml.TextAlignment.Left;
					else
						return UI.Xaml.TextAlignment.Right;
			}
		}
	}
}
