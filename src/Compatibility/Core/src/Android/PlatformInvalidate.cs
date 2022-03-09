using Microsoft.Maui.Controls.Internals;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.Android.PlatformInvalidate))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	class PlatformInvalidate : IPlatformInvalidate
	{
		public void Invalidate(VisualElement visualElement)
		{
			var renderer = visualElement.GetRenderer();
			if (renderer == null || renderer.View.IsDisposed())
			{
				return;
			}

			renderer.View.Invalidate();
			renderer.View.RequestLayout();
		}
	}
}