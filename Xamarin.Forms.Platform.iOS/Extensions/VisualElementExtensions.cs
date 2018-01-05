using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace Xamarin.Forms.Platform.iOS
{
	internal static class VisualElementExtensions
	{
		internal static bool UseLegacyColorManagement<T>(this T element) where T : VisualElement, IElementConfiguration<T>
		{
			// Determine whether we're letting the VSM handle the colors or doing it the old way
			// or disabling the legacy color management and doing it the old-old (pre 2.0) way
			return !element.HasVisualStateGroups()
					&& element.OnThisPlatform().GetIsLegacyColorModeEnabled();
		}
	}
}