using System;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.UI.Xaml;

namespace Microsoft.Maui.Controls.Platform
{
	public static class VisualElementExtensions
	{
		public static void UpdateAccessKey(this FrameworkElement platformView, IView view)
		{
			if (platformView is not null && view is VisualElement element)
				AccessKeyHelper.UpdateAccessKey(platformView, element);
		}

		internal static void Cleanup(this Element self)
		{
			if (self == null)
				throw new ArgumentNullException(nameof(self));

			foreach (Element element in self.Descendants())
			{
				if (element is Maui.IElement mauiElement)
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