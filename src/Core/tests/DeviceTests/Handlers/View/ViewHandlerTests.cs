using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.View)]
	public partial class ViewHandlerTests : CoreHandlerTestBase<StubBaseHandler, StubBase>
	{
		[Fact(DisplayName = "PlatformArrange triggers MapFrame")]
		public async Task PlatformArrangeTriggersMapFrame()
		{
			var didUpdateFrame = 0;

			var view = new StubBase();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = new StubBaseHandler();

				handler.CommandMapper.AppendToMapping(nameof(IView.Frame), (h, v, a) =>
				{
					didUpdateFrame++;
				});

				InitializeViewHandler(view, handler);
			});

			Assert.Equal(1, didUpdateFrame);
		}

		[Fact(DisplayName = "Subsequent PlatformArrange triggers MapFrame")]
		public async Task SubsequentPlatformArrangeTriggersMapFrame()
		{
			var didUpdateFrame = 0;

			var view = new StubBase();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = new StubBaseHandler();

				handler.CommandMapper.AppendToMapping(nameof(IView.Frame), (h, v, a) =>
				{
					didUpdateFrame++;
				});

				InitializeViewHandler(view, handler);

				handler.PlatformArrange(new Rect(0, 0, 100, 100));
			});

			Assert.Equal(2, didUpdateFrame);
		}

		const string ToolTipTextStringValue = "ToolTip Text Here!";

		[Theory]
		[InlineData(ToolTipTextStringValue)]
		[InlineData(null)]
		public Task ToolTipTextAppliedToLabel(string text)
			=> AssertTooltipTextApplied<LabelStub>(text);

		[Theory]
		[InlineData(ToolTipTextStringValue)]
		[InlineData(null)]
		public Task ToolTipTextAppliedToButton(string text)
			=> AssertTooltipTextApplied<ButtonStub>(text);

		[Theory]
		[InlineData(ToolTipTextStringValue)]
		[InlineData(null)]
		public Task ToolTipTextAppliedToImage(string text)
			=> AssertTooltipTextApplied<ImageStub>(text);

		[Theory]
		[InlineData(ToolTipTextStringValue)]
		[InlineData(null)]
		public Task ToolTipTextAppliedToCheckbox(string text)
			=> AssertTooltipTextApplied<CheckBoxStub>(text);

		async Task AssertTooltipTextApplied<TElement>(string expected)
			where TElement : StubBase
		{
#if ANDROID
			// Tooltips are only supported in the Android API as of 26+
			// Prior to this, the TooltipCompat.SetToolTipText emulates the behavior
			// however there is no TooltipCompat.GetToolTipText equivalent, so there's
			// no way to actually 'get' the text that's currently set
			// For now there's no valid way to test these cases, so we'll skip
			if (!OperatingSystem.IsAndroidVersionAtLeast(26))
				return;
#endif

#if IOS || MACCATALYST
			// ToolTips are only available on iOS 15+
			if (!(OperatingSystem.IsMacCatalystVersionAtLeast(15) || OperatingSystem.IsIOSVersionAtLeast(15)))
				return;
#endif
			var control = (TElement)Activator.CreateInstance(typeof(TElement));
			control.ToolTip = new ToolTip() { Content = expected };

			var handler = await CreateHandlerAsync(control);

			var platformText = await InvokeOnMainThreadAsync(() =>
			{
#if IOS || MACCATALYST
				return handler.ToPlatform()?.GetToolTipInteraction()?.DefaultToolTip;
#elif ANDROID
				return handler.ToPlatform()?.TooltipText;
#elif WINDOWS
				return Microsoft.UI.Xaml.Controls.ToolTipService.GetToolTip(handler.ToPlatform()) as string;
#else
				return null;
#endif
			});

			Assert.Equal(expected, platformText);
		}
	}
}
