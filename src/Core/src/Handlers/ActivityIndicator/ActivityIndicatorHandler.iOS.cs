using System;
using System.Runtime.Versioning;
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

			UIActivityIndicatorViewStyle style;
			if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
				style = MediumStyle;
			else
				style = UIActivityIndicatorViewStyle.Gray;
			platformView = new MauiActivityIndicator(CGRect.Empty, VirtualView) { ActivityIndicatorViewStyle = style };

			return platformView;
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.1")]
		static UIActivityIndicatorViewStyle MediumStyle => UIActivityIndicatorViewStyle.Medium;


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