using Gtk;

namespace Microsoft.Maui
{
	public static class AlignmentExtensions
	{
		public static Align ToNative(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Start:
					return Align.Start;
				case TextAlignment.End:
					return Align.End;
				default:
					return Align.Center;
			}
		}
	}
}
