using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class SwitchHandlerTests
	{
		UISwitch GetNativeSwitch(SwitchHandler switchHandler) =>
			(UISwitch)switchHandler.PlatformView;

		// This will not fire a ValueChanged event on native
		void SetIsOn(SwitchHandler switchHandler, bool value) =>
			switchHandler.PlatformView.SetState(value, true);

		bool GetNativeIsOn(SwitchHandler switchHandler) =>
		  GetNativeSwitch(switchHandler).On;

		async Task ValidateTrackColor(ISwitch switchStub, Color color, Action action = null)
		{
			var expected = await GetValueAsync(switchStub, handler =>
			{
				var native = GetNativeSwitch(handler);
				action?.Invoke();
				return native.OnTintColor.ToColor();
			});
			Assert.Equal(expected, color);
		}

		async Task ValidateThumbColor(ISwitch switchStub, Color color, Action action = null)
		{
			var expected = await GetValueAsync(switchStub, handler =>
			{
				var native = GetNativeSwitch(handler);
				action?.Invoke();
				return native.ThumbTintColor.ToColor();
			});

			Assert.Equal(expected, color);
		}

		async Task ValidateVisualTrackColor(ISwitch switchStub, UIColor color)
		{
			var actualBackgroundColor = await GetValueAsync(switchStub, handler =>
			{
				var uISwitch = GetNativeSwitch(handler);
				var uIView = uISwitch.GetTrackSubview();
				return uIView?.BackgroundColor;
			});

			Assert.NotNull(actualBackgroundColor);

			// Compare the actual RGBA values since UIColor can be picky
			actualBackgroundColor.GetRGBA (out var actualRed, out var actualGreen, out var actualBlue, out var actualAlpha);
			color.GetRGBA (out var colorRed, out var colorGreen, out var colorBlue, out var colorAlpha);

			Assert.True(actualRed == colorRed);
			Assert.True(actualGreen == colorGreen);
			Assert.True(actualBlue == colorBlue);
			Assert.True(actualAlpha == colorAlpha);
		}

		async Task ValidateTrackSubViewExists(ISwitch switchStub)
		{
			var uIView = await GetValueAsync(switchStub, handler =>
			{
				var uISwitch = GetNativeSwitch(handler);
				return uISwitch.GetTrackSubview();
			});

			Assert.NotNull(uIView);

		/// <summary>
		/// If a UISwitch grows beyond 101 pixels it's no longer
		/// clickable via Voice Over
		/// </summary>
		/// <returns></returns>
		[Fact(DisplayName = "Ensure UISwitch Stays Below 101 Width")]
		public async Task EnsureUISwitchStaysBelow101Width()
		{
			var switchStub = new SwitchStub()
			{
				Width = 400,
				Height = 400
			};

			var width = await GetValueAsync(switchStub, handler => GetNativeSwitch(handler).Bounds.Width);

			Assert.True(width < 100, $"UISwitch width is too much {width}");
		}
	}
}
