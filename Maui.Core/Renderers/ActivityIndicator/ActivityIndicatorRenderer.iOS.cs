using CoreGraphics;
using UIKit;

namespace System.Maui.Platform
{
	public partial class ActivityIndicatorRenderer : AbstractViewRenderer<IActivityIndicator, UIActivityIndicatorView>
	{
		protected override UIActivityIndicatorView CreateView()
		{
#if __XCODE11__
			if(NativeVersion.Supports(NativeApi.UIActivityIndicatorViewStyleMedium))
				return new UIActivityIndicatorView(CGRect.Empty) { ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Medium };
			else
#endif
			return new UIActivityIndicatorView(CGRect.Empty) { ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray };
		}

		public static void MapPropertyIsRunning(IViewRenderer renderer, IActivityIndicator activityIndicator)
		{
			if (!(renderer.NativeView is UIActivityIndicatorView uIActivityIndicatorView))
				return;

			if (activityIndicator.IsRunning)
				uIActivityIndicatorView.StartAnimating();
			else
				uIActivityIndicatorView.StopAnimating();
		}

		public static void MapPropertyColor(IViewRenderer renderer, IActivityIndicator activityIndicator)
		{
			if (!(renderer.NativeView is UIActivityIndicatorView uIActivityIndicatorView))
				return;

			if (!uIActivityIndicatorView.IsAnimating && activityIndicator.IsRunning)
				uIActivityIndicatorView.StartAnimating();
		}
	}
}