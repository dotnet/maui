using Android.Views;

namespace Xamarin.Forms.Platform.Android
{
	internal static class AlignmentExtensions
	{
		internal static GravityFlags ToHorizontalGravityFlags(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Center:
					return GravityFlags.CenterHorizontal;
				case TextAlignment.End:
					return GravityFlags.Right;
				default:
					return GravityFlags.Left;
			}
		}

		internal static GravityFlags ToVerticalGravityFlags(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Start:
					return GravityFlags.Top;
				case TextAlignment.End:
					return GravityFlags.Bottom;
				default:
					return GravityFlags.CenterVertical;
			}
		}
	}
}