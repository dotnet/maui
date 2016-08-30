#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public static class PlatformConfigurationExtensions
	{
		public static IPlatformElementConfiguration<PlatformConfiguration.Windows, T> OnThisPlatform<T>(this T element) 
			where T : Element, IElementConfiguration<T>
		{
			return (element).On<PlatformConfiguration.Windows>();
		}
	}
}
