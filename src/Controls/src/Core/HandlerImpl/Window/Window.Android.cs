#nullable enable
using System;
using Android.App;
using Android.Views;
using AndroidX.AppCompat.App;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.Controls
{
	public partial class Window
	{
		internal Activity PlatformActivity =>
			(Handler?.PlatformView as Activity) ?? throw new InvalidOperationException("Window should have an Activity set.");

		[Obsolete]
		public static void MapContent(WindowHandler handler, IWindow view)
		{
		}

		public static void MapContent(IWindowHandler handler, IWindow view)
		{
		}
	}
}