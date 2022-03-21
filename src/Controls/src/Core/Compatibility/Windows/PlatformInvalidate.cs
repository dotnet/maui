using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.UWP.PlatformInvalidate))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	class PlatformInvalidate : IPlatformInvalidate
	{
		public void Invalidate(VisualElement visualElement)
		{
			if (visualElement.Handler?.PlatformView == null)
				return;

			visualElement.ToPlatform().InvalidateMeasure();
		}
	}
}