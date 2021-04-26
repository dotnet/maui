using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class FrameHandler : ViewHandler<IFrame, Border>
	{
		protected override Border CreateNativeView()
		{
			return new Border();
		}
		public static void MapContent(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateContent(frame, handler.MauiContext);
		}

		public static void MapBackgroundColor(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateBackgroundColor(frame);
		}

		public static void MapBorderColor(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateBorderColor(frame);
		}

		[MissingMapper]
		public static void MapHasShadow(FrameHandler handler, IFrame frame) { }

		public static void MapCornerRadius(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateCornerRadius(frame);
		}
	}
}