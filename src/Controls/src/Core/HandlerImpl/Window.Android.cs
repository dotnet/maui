#nullable enable
using System;
using Android.App;
using AndroidX.AppCompat.App;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		internal Activity NativeActivity =>
			(Handler?.NativeView as Activity) ?? throw new InvalidOperationException("Window should have an Activity set.");
	}
}