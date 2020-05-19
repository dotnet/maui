using System;
using ElmSharp;

namespace System.Maui
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
