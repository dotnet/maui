using System;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Microsoft.Maui.Controls.Platform
{
	public static class VisualElementExtensions
	{
		public static IViewHandler GetOrCreateHandler(this IView self, IMauiContext mauiContext)
		{
			if (self == null)
				throw new ArgumentNullException("self");

			if (self.Handler != null)
				return self.Handler;

			self.ToNative(mauiContext);
			return self.Handler;
		}

		// TODO MAUI Make this work against IVIEW
		internal static void Cleanup(this VisualElement self)
		{
			if (self == null)
				throw new ArgumentNullException("self");

			IViewHandler handler = self.Handler;

			foreach (Element element in self.Descendants())
			{
				var visual = element as VisualElement;
				if (visual == null)
					continue;

				visual.Handler = null;
			}

			self.Handler = null;
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