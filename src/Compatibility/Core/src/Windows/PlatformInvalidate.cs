using Microsoft.Maui.Controls.Internals;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.UWP.PlatformInvalidate))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	class PlatformInvalidate : IPlatformInvalidate
	{
		public void Invalidate(VisualElement visualElement)
		{
			var renderer = Platform.GetRenderer(visualElement);
			if (renderer == null)
			{
				return;
			}

			renderer.ContainerElement.InvalidateMeasure();
		}
	}
}