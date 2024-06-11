using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LayoutTests
	{
		void ValidateInputTransparentOnPlatformView(IView view)
		{
			var handler = view.ToHandler(MauiContext);
			if (handler.PlatformView is LayoutViewGroup lvg)
			{
				Assert.Equal(view.InputTransparent, lvg.InputTransparent);
				if (handler.ContainerView is WrapperView wv)
					Assert.False(wv.InputTransparent);

				return;
			}

			if (view.InputTransparent)
			{
				Assert.True(view.ToPlatform(MauiContext) is WrapperView wv && wv.InputTransparent);
			}
			else
			{
				Assert.True(view.ToPlatform(MauiContext) is not WrapperView wv || !wv.InputTransparent);
			}
		}
	}
}
