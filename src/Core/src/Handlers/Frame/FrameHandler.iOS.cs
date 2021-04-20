using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class FrameHandler : ViewHandler<IFrame, UIView>
	{
		static MauiFrame? ActualView;

		protected override UIView CreateNativeView()
		{
			return new UIView();
		}

		protected override void ConnectHandler(UIView nativeView)
		{
			ActualView = new MauiFrame();
			nativeView.AddSubview(ActualView);
		}

		protected override void DisconnectHandler(UIView nativeView)
		{
			if (ActualView != null)
			{
				ActualView.RemoveFromSuperview();
				ActualView.Dispose();
				ActualView = null;
			}
		}

		public static void MapContent(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateContent(frame, handler.MauiContext);
		}

		public static void MapBackgroundColor(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateBackgroundColor(frame, ActualView);
		}

		public static void MapBorderColor(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateBorderColor(frame, ActualView);
		}

		public static void MapHasShadow(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateHasShadow(frame, ActualView);
		}

		public static void MapCornerRadius(FrameHandler handler, IFrame frame)
		{
			handler.NativeView?.UpdateCornerRadius(frame, ActualView);
		}
	}
}