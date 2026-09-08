using System;

namespace Microsoft.Maui.Controls.Compatibility
{
	public class ViewInitializedEventArgs : EventArgs
	{
		public global::Android.Views.View NativeView { get; internal set; }

		public VisualElement View { get; internal set; }
	}
}