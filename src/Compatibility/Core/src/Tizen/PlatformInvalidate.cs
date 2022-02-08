using Microsoft.Maui.Controls.Internals;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.Tizen.PlatformInvalidate))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	class PlatformInvalidate : IPlatformInvalidate
	{
		public void Invalidate(VisualElement visualElement)
		{
			var renderer = Microsoft.Maui.Controls.Compatibility.Platform.Tizen.Platform.GetRenderer(visualElement);
			if (renderer == null || !renderer.NativeView.IsRealized)
			{
				return;
			}

			renderer.NativeView.MarkChanged();
		}
	}
}