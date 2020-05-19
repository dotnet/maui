using System;
using ALayoutChangeEventArgs = Android.Views.View.LayoutChangeEventArgs;

namespace System.Maui.Platform.Android
{
	internal interface ILayoutChanges
	{
		event EventHandler<ALayoutChangeEventArgs> LayoutChange;
		bool HasLayoutOccurred { get;  }
	}
}