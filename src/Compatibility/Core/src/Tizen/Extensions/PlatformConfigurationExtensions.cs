using CurrentPlatform = Microsoft.Maui.Controls.Compatibility.PlatformConfiguration.Tizen;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
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
