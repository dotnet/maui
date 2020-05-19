using Tizen.Multimedia;
using Xamarin.Forms.PlatformConfiguration.TizenSpecific;
using ERect = ElmSharp.Rect;
using MRect = Tizen.Multimedia.Rectangle;

namespace Xamarin.Forms.Platform.Tizen
{
	public static class MediaElementExtension
	{
		public static MRect ToMultimediaRectangle(this ERect rect)
		{
			return new MRect(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static PlayerDisplayMode ToNative(this Aspect aspect)
		{
			PlayerDisplayMode ret = PlayerDisplayMode.LetterBox;
			switch (aspect)
			{
				case Aspect.AspectFill:
					ret = PlayerDisplayMode.CroppedFull;
					break;
				case Aspect.AspectFit:
					ret = PlayerDisplayMode.LetterBox;
					break;
				case Aspect.Fill:
					ret = PlayerDisplayMode.FullScreen;
					break;
			}
			return ret;
		}

		public static PlayerDisplayMode ToNative(this DisplayAspectMode mode)
		{
			PlayerDisplayMode ret = PlayerDisplayMode.LetterBox;
			switch (mode)
			{
				case DisplayAspectMode.AspectFill:
					ret = PlayerDisplayMode.CroppedFull;
					break;
				case DisplayAspectMode.AspectFit:
					ret = PlayerDisplayMode.LetterBox;
					break;
				case DisplayAspectMode.Fill:
					ret = PlayerDisplayMode.FullScreen;
					break;
				case DisplayAspectMode.OrignalSize:
					ret = PlayerDisplayMode.OriginalOrFull;
					break;
			}
			return ret;
		}

		public static DisplayAspectMode ToDisplayAspectMode(this Aspect aspect)
		{
			DisplayAspectMode ret = DisplayAspectMode.AspectFit;
			switch (aspect)
			{
				case Aspect.AspectFill:
					ret = DisplayAspectMode.AspectFill;
					break;
				case Aspect.AspectFit:
					ret = DisplayAspectMode.AspectFit;
					break;
				case Aspect.Fill:
					ret = DisplayAspectMode.Fill;
					break;
			}
			return ret;
		}
	}
}
