using System;
using ALayoutChangeEventArgs = Android.Views.View.LayoutChangeEventArgs;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal interface ILayoutChanges
	{
		event EventHandler<ALayoutChangeEventArgs> LayoutChange;
		bool HasLayoutOccurred { get; }
	}
}