using System;
using ElmSharp;

namespace Microsoft.Maui.Controls.Compatibility
{
	public class ViewInitializedEventArgs : EventArgs
	{
		public EvasObject NativeView
		{
			get;
			internal set;
		}

		public VisualElement View
		{
			get;
			internal set;
		}
	}
}
