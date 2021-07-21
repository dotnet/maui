using System;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public static class VisualElementExtensions
	{
		public static IVisualElementRenderer GetOrCreateRenderer(this VisualElement self)
		{
			if (self == null)
				throw new ArgumentNullException("self");

			IVisualElementRenderer renderer = Platform.GetRenderer(self);
			if (renderer == null)
			{
				renderer = Platform.CreateRenderer(self);
				Platform.SetRenderer(self, renderer);
			}

			return renderer;
		}

		internal static void Cleanup(this VisualElement self)
		{
			if (self == null)
				throw new ArgumentNullException("self");

			IVisualElementRenderer renderer = Platform.GetRenderer(self);

			foreach (Element element in self.Descendants())
			{
				var visual = element as VisualElement;
				if (visual == null)
					continue;

				IVisualElementRenderer childRenderer = Platform.GetRenderer(visual);
				if (childRenderer != null)
				{
					childRenderer.Dispose();
					Platform.SetRenderer(visual, null);
				}
			}

			if (renderer != null)
			{
				renderer.Dispose();
				Platform.SetRenderer(self, null);
			}
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