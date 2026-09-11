#if MACCATALYST
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Gesture)]
	public class ReplicationGesturePlatformManagerRegression : ControlsHandlerTestBase
	{
		[Fact]
		public async Task SecondaryToPrimaryCreatesNativeTap()
		{
			var label = new Label();
			var tapGestureRecognizer = new TapGestureRecognizer
			{
				Buttons = ButtonsMask.Secondary,
				NumberOfTapsRequired = 2
			};
			label.GestureRecognizers.Add(tapGestureRecognizer);

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<LabelHandler>(label);
				try
				{
					var nativeTaps = handler.PlatformView.GestureRecognizers?
						.OfType<UITapGestureRecognizer>()
						.Where(tap => tap.NumberOfTapsRequired == 2)
						.ToArray() ?? Array.Empty<UITapGestureRecognizer>();

					Assert.Empty(nativeTaps);

					tapGestureRecognizer.Buttons = ButtonsMask.Primary;

					nativeTaps = handler.PlatformView.GestureRecognizers?
						.OfType<UITapGestureRecognizer>()
						.Where(tap => tap.NumberOfTapsRequired == 2)
						.ToArray() ?? Array.Empty<UITapGestureRecognizer>();

					Assert.Single(nativeTaps);
				}
				finally
				{
					((IElementHandler)handler).DisconnectHandler();
				}
			});
		}

		[Fact]
		public async Task SecondaryToBothCreatesNativeTap()
		{
			var label = new Label();
			var tapGestureRecognizer = new TapGestureRecognizer
			{
				Buttons = ButtonsMask.Secondary,
				NumberOfTapsRequired = 2
			};
			label.GestureRecognizers.Add(tapGestureRecognizer);

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<LabelHandler>(label);
				try
				{
					var nativeTaps = handler.PlatformView.GestureRecognizers?
						.OfType<UITapGestureRecognizer>()
						.Where(tap => tap.NumberOfTapsRequired == 2)
						.ToArray() ?? Array.Empty<UITapGestureRecognizer>();

					Assert.Empty(nativeTaps);

					tapGestureRecognizer.Buttons = ButtonsMask.Primary | ButtonsMask.Secondary;

					nativeTaps = handler.PlatformView.GestureRecognizers?
						.OfType<UITapGestureRecognizer>()
						.Where(tap => tap.NumberOfTapsRequired == 2)
						.ToArray() ?? Array.Empty<UITapGestureRecognizer>();

					Assert.Single(nativeTaps);
				}
				finally
				{
					((IElementHandler)handler).DisconnectHandler();
				}
			});
		}
	}
}
#endif
