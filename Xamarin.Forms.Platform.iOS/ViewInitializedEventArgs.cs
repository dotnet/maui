using System;
using UIKit;

namespace Xamarin.Forms
{
	public class ViewInitializedEventArgs : EventArgs
	{
		public UIView NativeView { get; internal set; }

		public VisualElement View { get; internal set; }
	}
}