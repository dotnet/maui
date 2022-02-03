using System;

namespace Microsoft.Maui.Controls.Compatibility
{
	public class ViewInitializedEventArgs : EventArgs
	{
		public global::Android.Views.View PlatformView { get; internal set; }

		public VisualElement View { get; internal set; }
	}
}