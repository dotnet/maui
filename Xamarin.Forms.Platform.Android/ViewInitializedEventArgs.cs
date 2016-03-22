using System;

namespace Xamarin.Forms
{
	public class ViewInitializedEventArgs : EventArgs
	{
		public global::Android.Views.View NativeView { get; internal set; }

		public VisualElement View { get; internal set; }
	}
}