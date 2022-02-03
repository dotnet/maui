using System;
#if __MOBILE__
using TPlatformView = UIKit.UIView;
#else
using TPlatformView = AppKit.NSView;

#endif

namespace Microsoft.Maui.Controls.Compatibility
{
	public class ViewInitializedEventArgs : EventArgs
	{
		public TPlatformView PlatformView { get; internal set; }

		public VisualElement View { get; internal set; }
	}
}