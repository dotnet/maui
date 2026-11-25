using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class StoryboardTests
	{
		static partial void AssertNativeTransformCore(IViewHandler handler, double translationX, double scale)
		{
			var view = (UIKit.UIView)handler.ToPlatform();
			var transform = view.Layer.Transform;

			Assert.Equal(translationX, transform.M41, 2);
			Assert.Equal(scale, transform.M11, 2);
		}
	}
}
