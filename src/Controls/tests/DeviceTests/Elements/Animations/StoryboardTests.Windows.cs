using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class StoryboardTests
	{
		static partial void AssertNativeTransformCore(IViewHandler handler, double translationX, double scale)
		{
			var view = (FrameworkElement)handler.ToPlatform();
			var transform = Assert.IsType<CompositeTransform>(view.RenderTransform);

			Assert.Equal(translationX, transform.TranslateX, 2);
			Assert.Equal(scale, transform.ScaleX, 2);
		}
	}
}
