#if __MOBILE__
using CurrentPlatform = System.Maui.PlatformConfiguration.iOS;
namespace System.Maui.Platform.iOS
#else
using CurrentPlatform = System.Maui.PlatformConfiguration.macOS;

namespace System.Maui.Platform.MacOS
#endif
{
	public static class PlatformConfigurationExtensions
	{
		public static IPlatformElementConfiguration<CurrentPlatform, T> OnThisPlatform<T>(this T element)
			where T : Element, IElementConfiguration<T>
		{
			return (element).On<CurrentPlatform>();
		}
	}
}