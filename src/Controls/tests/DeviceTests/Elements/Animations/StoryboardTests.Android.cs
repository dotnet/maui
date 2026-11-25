using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class StoryboardTests
	{
		static partial void AssertNativeTransformCore(IViewHandler handler, double translationX, double scale)
		{
			var view = (global::Android.Views.View)handler.ToPlatform();

			Assert.Equal(view.ToPixels(translationX), view.TranslationX, 1);
			Assert.Equal(scale, view.ScaleX, 2);
			Assert.Equal(scale, view.ScaleY, 2);
		}
	}
}
