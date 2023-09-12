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
			MauiActivityIndicator platformView;

			if (OperatingSystem.IsIOSVersionAtLeast(13))
				platformView = new MauiActivityIndicator(CGRect.Empty, VirtualView) { ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Medium };
			else
				platformView = new MauiActivityIndicator(CGRect.Empty, VirtualView) { ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray };

			return platformView;
		}

		public static partial void MapIsRunning(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateIsRunning(activityIndicator);
		}

		public static partial void MapColor(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
		{
			handler.PlatformView?.UpdateColor(activityIndicator);
		}
	}
}