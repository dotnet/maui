using Microsoft.Maui.Controls.Internals;

[assembly: Microsoft.Maui.Controls.Dependency(typeof(Microsoft.Maui.Controls.Compatibility.Platform.iOS.PlatformSizeService))]

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	class PlatformSizeService : IPlatformSizeService
	{
		public SizeRequest GetPlatformSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			if (widthConstraint > 0 && heightConstraint > 0)
			{
				return view.Handler?.GetDesiredSize(widthConstraint, heightConstraint) ??
					new SizeRequest();
			}

			return new SizeRequest();
		}
	}
}