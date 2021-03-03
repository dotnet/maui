#if __MOBILE__
using CurrentPlatform = Microsoft.Maui.Controls.PlatformConfiguration.iOS;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using CurrentPlatform = Microsoft.Maui.Controls.Compatibility.PlatformConfiguration.macOS;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
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