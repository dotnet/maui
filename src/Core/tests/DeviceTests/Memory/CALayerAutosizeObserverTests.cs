#if IOS || MACCATALYST

using System;
using System.Threading.Tasks;
using CoreAnimation;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Memory
{
	// Set of tests to verify auto-sizing layers do not leak
	[Category(TestCategory.Memory)]
	public class CALayerAutosizeObserverTests : TestBase
	{
		[Theory]
		[InlineData(typeof(MauiCALayer))]
		[InlineData(typeof(StaticCALayer))]
		[InlineData(typeof(StaticCAGradientLayer))]
		[InlineData(typeof(StaticCAShapeLayer))]
		public async Task CALayerAutosizeObserver_DoesNotLeak(Type sublayerType)
		{
			WeakReference viewReference = null;
			WeakReference layerReference = null;
			WeakReference sublayerReference = null;

			await InvokeOnMainThreadAsync(() =>
			{
				var view = new UIView();
				viewReference = new(view);

				layerReference = new(view.Layer);

				var sublayer = (IAutoSizableCALayer)Activator.CreateInstance(sublayerType)!;
				sublayerReference = new(sublayer);

				view.Layer.AddSublayer((CALayer)sublayer);

				sublayer.AutoSizeToSuperLayer();
				view.Frame = new CoreGraphics.CGRect(0, 0, 100, 100);
			});

			await AssertionExtensions.WaitForGC(viewReference, layerReference, sublayerReference);
		}
	}
}

#endif