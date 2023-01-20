using Tizen.UIExtensions.Common;
using TScrollView = Tizen.UIExtensions.NUI.ScrollView;
using TSize = Tizen.UIExtensions.Common.Size;

namespace Microsoft.Maui.Platform
{
	public class MauiScrollView : TScrollView, IMeasurable
	{
		IScrollView _virtualView;

		public MauiScrollView(IScrollView virtualView)
		{
			_virtualView = virtualView;
		}

		public TSize Measure(double availableWidth, double availableHeight)
		{
			return _virtualView.CrossPlatformMeasure(availableWidth.ToScaledDP(), availableHeight.ToScaledDP()).ToPixel();
		}
	}
}