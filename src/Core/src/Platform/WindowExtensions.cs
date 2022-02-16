using System.Collections.Generic;

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
