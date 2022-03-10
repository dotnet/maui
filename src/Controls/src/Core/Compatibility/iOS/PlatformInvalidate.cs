using Microsoft.Maui.Controls.Internals;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.iOS.PlatformInvalidate))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	class PlatformInvalidate : IPlatformInvalidate
	{
		public void Invalidate(VisualElement visualElement)
		{
			if (visualElement.Handler?.PlatformView == null)
				return;

			visualElement.ToPlatform().SetNeedsLayout();
		}
	}
}