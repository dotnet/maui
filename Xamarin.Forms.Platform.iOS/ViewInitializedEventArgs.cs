using System;
#if __UNIFIED__
using UIKit;

#else
using MonoTouch.UIKit;
#endif

namespace Xamarin.Forms
{
	public class ViewInitializedEventArgs : EventArgs
	{
		public UIView NativeView { get; internal set; }

		public VisualElement View { get; internal set; }
	}
}