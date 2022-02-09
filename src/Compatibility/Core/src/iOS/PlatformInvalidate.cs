using Microsoft.Maui.Controls.Internals;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.iOS.PlatformInvalidate))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	class PlatformInvalidate : IPlatformInvalidate
	{
		public void Invalidate(VisualElement visualElement)
		{
			var renderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform.GetRenderer(visualElement);

			if (renderer == null)
			{
				return;
			}

			renderer.PlatformView.SetNeedsLayout();
		}
	}
}