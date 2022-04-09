using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Media;

#if __IOS__ || MACCATALYST
using PlatformView = UIKit.UIWindow;
#elif MONOANDROID
using PlatformView = Android.App.Activity;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Window;
#endif

namespace Microsoft.Maui.Platform
{
	public static partial class WindowExtensions
	{
#if !NETSTANDARD2_0
		internal static IReadOnlyList<IWindow> GetWindows()
		{
			if (IPlatformApplication.Current is not IPlatformApplication platformApplication)
				return new List<IWindow>();

			if (platformApplication.Application is not IApplication application)
				return new List<IWindow>();

			return application.Windows;
		}
#endif
	}
}
