using System;
using ElmSharp;

namespace Xamarin.Forms
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
