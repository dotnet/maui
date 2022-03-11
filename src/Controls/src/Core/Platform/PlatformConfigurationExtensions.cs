#if __IOS__ || MACCATALYST
using CurrentPlatform = Microsoft.Maui.Controls.PlatformConfiguration.iOS;
#elif __ANDROID__
using CurrentPlatform = Microsoft.Maui.Controls.PlatformConfiguration.Android;
#elif WINDOWS
using CurrentPlatform = Microsoft.Maui.Controls.PlatformConfiguration.Windows;
#elif NETSTANDARD
using PlatformView = System.Object;
#endif


#if !NETSTANDARD
namespace Microsoft.Maui.Controls.Platform
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
#endif