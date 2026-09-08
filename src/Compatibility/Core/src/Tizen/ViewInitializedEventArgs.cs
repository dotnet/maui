using System;
using NView = Tizen.NUI.BaseComponents.View;

namespace Microsoft.Maui.Controls.Compatibility
{
	public class ViewInitializedEventArgs : EventArgs
	{
		public NView NativeView
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
