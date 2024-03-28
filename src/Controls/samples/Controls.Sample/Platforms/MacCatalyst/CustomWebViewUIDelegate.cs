using System;
using Foundation;
using WebKit;


namespace Maui.Controls.Sample.Platforms.MacCatalyst
{
	public class CustomWebViewUIDelegate : WKUIDelegate
	{
		public CustomWebViewUIDelegate()
		{

		}

		[Export("webView:requestMediaCapturePermissionForOrigin:initiatedByFrame:type:decisionHandler:")]
		public override void RequestMediaCapturePermission(WKWebView webView, WKSecurityOrigin origin, WKFrameInfo frame, WKMediaCaptureType type, Action<WKPermissionDecision> decisionHandler)
		{
			decisionHandler(WKPermissionDecision.Grant);
		}
		public override void RequestDeviceOrientationAndMotionPermission(WKWebView webView, WKSecurityOrigin origin, WKFrameInfo frame, Action<WKPermissionDecision> decisionHandler)
		{
			base.RequestDeviceOrientationAndMotionPermission(webView, origin, frame, decisionHandler);
		}
	}
}