using EScrollToPosition = ElmSharp.ScrollToPosition;

namespace Xamarin.Forms.Platform.Tizen
{
	public static class ScrollToPositionExtensions
	{
		public static EScrollToPosition ToNative(this ScrollToPosition position)
		{
			switch (position)
			{
				case ScrollToPosition.Center:
					return EScrollToPosition.Middle;

				case ScrollToPosition.End:
					return EScrollToPosition.Bottom;

				case ScrollToPosition.MakeVisible:
					return EScrollToPosition.In;

				case ScrollToPosition.Start:
					return EScrollToPosition.Top;

				default:
					return EScrollToPosition.Top;
			}
		}
	}
}
