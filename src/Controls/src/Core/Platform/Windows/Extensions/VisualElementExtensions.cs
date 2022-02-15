#nullable enable
using System;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Microsoft.Maui.Controls.Platform
{
	public static class VisualElementExtensions
	{
		internal static void Cleanup(this Element self)
		{
			if (self == null)
				throw new ArgumentNullException("self");

			foreach (Element element in self.Descendants())
			{
				if(element is Maui.IElement mauiElement)
					mauiElement.Handler?.DisconnectHandler();
			}

			self.Handler?.DisconnectHandler();
		}

		internal static bool UseFormsVsm<T>(this T element) where T : VisualElement, IElementConfiguration<T>
		{
			// Determine whether we're letting the VSM handle the colors or doing it the old way
			// or disabling the legacy color management and doing it the old-old (pre 2.0) way
			return element.HasVisualStateGroups()
					|| !element.OnThisPlatform().GetIsLegacyColorModeEnabled();
		}
	}
}