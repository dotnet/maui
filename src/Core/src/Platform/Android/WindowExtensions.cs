using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Media;

namespace Microsoft.Maui.Platform
{
	public static partial class WindowExtensions
	{
		public static async Task<RenderedView?> RenderAsImage(this IWindow window, RenderType type)
		{
			if (window.Handler?.PlatformView is not Activity activity)
				return null;

			if (activity?.Window?.DecorView?.RootView is not View rootView)
				return null;

			var image = type switch
			{
				RenderType.JPEG => await rootView.RenderAsJPEG(),
				RenderType.PNG => await rootView.RenderAsPNG(),
				RenderType.BMP => rootView.RenderAsBMP(),
				_ => throw new NotImplementedException(),
			};

			return new RenderedView(image, type);
		}
	}
}
