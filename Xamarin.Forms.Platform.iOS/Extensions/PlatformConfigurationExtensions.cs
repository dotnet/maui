#if __MOBILE__
using CurrentPlatform = Xamarin.Forms.PlatformConfiguration.iOS;
namespace Xamarin.Forms.Platform.iOS
#else
using CurrentPlatform = Xamarin.Forms.PlatformConfiguration.macOS;

namespace Xamarin.Forms.Platform.MacOS
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