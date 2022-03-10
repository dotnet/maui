using Microsoft.Maui.Controls.Internals;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.Android.PlatformInvalidate))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	class PlatformInvalidate : IPlatformInvalidate
	{
		public void Invalidate(VisualElement visualElement)
		{
			if (visualElement.Handler?.PlatformView == null)
				return;

			var view = visualElement.ToPlatform();

			view.Invalidate();
			view.RequestLayout();
		}
	}
}