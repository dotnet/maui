using System;
using CoreGraphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Handlers
{
	public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, MauiActivityIndicator>
	{
		protected override MauiActivityIndicator CreatePlatformView()
		{
			var activityIndicator = new MauiActivityIndicator(CGRect.Empty, VirtualView);
			if (!OperatingSystem.IsIOSVersionAtLeast(13)) // 'UIActivityIndicatorViewStyle.Gray' is unsupported on: 'ios' 13.0 and later
				activityIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray;
			return activityIndicator;
		}

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