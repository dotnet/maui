using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, MauiActivityIndicator>
	{
		protected override MauiActivityIndicator CreatePlatformView() => new MauiActivityIndicator(CGRect.Empty, VirtualView)
		{
#pragma warning disable CA1416 // TODO: 'UIActivityIndicatorViewStyle.Gray' is unsupported on: 'ios' 13.0 and later
			ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray
#pragma warning restore CA1416
		};

		public static void MapIsRunning(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateIsRunning(activityIndicator);
		}

		public static void MapColor(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateColor(activityIndicator);
		}
	}
}