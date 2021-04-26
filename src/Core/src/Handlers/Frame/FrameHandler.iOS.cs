using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class FrameHandler : ViewHandler<IFrame, MauiFrame>
	{
		static MauiFrameContent? Content;

		protected override MauiFrame CreateNativeView()
		{
			return new MauiFrame();
		}

		protected override void ConnectHandler(MauiFrame nativeView)
		{
			Content = new MauiFrameContent();
			nativeView.AddSubview(Content);
		}

		protected override void DisconnectHandler(MauiFrame nativeView)
		{
			if (Content != null)
			{
				Content.RemoveFromSuperview();
				Content.Dispose();
				Content = null;
			}
		}

		public static void MapContent(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateContent(frame, handler.MauiContext);
		}

		public static void MapBackgroundColor(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateBackgroundColor(frame, Content);
		}

		public static void MapBorderColor(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateBorderColor(frame, Content);
		}

		public static void MapHasShadow(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateHasShadow(frame, Content);
		}

		public static void MapCornerRadius(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateCornerRadius(frame, Content);
		}
	}
}