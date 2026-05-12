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

		async Task ValidateTrackColor(ISwitch switchStub, Color color, Action action = null, string updatePropertyValue = null)
		{
			var expected = await GetValueAsync(switchStub, handler =>
			{
				var native = GetNativeSwitch(handler);
				action?.Invoke();
				if (!string.IsNullOrEmpty(updatePropertyValue))
				{
					handler.UpdateValue(updatePropertyValue);
				}

				return native.OnTintColor.ToColor();
			});
			Assert.Equal(expected, color);
		}

		async Task ValidateThumbColor(ISwitch switchStub, Color color, Action action = null, string updatePropertyValue = null)
		{
			var expected = await GetValueAsync(switchStub, handler =>
			{
				var native = GetNativeSwitch(handler);
				action?.Invoke();
				if (!string.IsNullOrEmpty(updatePropertyValue))
				{
					handler.UpdateValue(updatePropertyValue);
				}

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
			actualBackgroundColor.GetRGBA(out var actualRed, out var actualGreen, out var actualBlue, out var actualAlpha);
			color.GetRGBA(out var colorRed, out var colorGreen, out var colorBlue, out var colorAlpha);

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
		}

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

		[Fact(DisplayName = "Track Color's view is set when toggled on")]
		public async Task OnTrackColorSetVisually()
		{
			var switchStub = new SwitchStub()
			{
				IsOn = false,
				TrackColor = Colors.Red,
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				await ValidatePropertyUpdatesValue(
					view: switchStub,
					property: nameof(ISwitch.IsOn),
					GetPlatformValue: (handler) => handler.PlatformView.On,
					expectedSetValue: true,
					expectedUnsetValue: false);

				await ValidateVisualTrackColor(switchStub, UIColor.Red);
			});
		}

		[Fact(DisplayName = "Track Color's view is default color when toggled off")]
		public async Task OffTrackColorSetToDefaultColor()
		{
			var switchStub = new SwitchStub()
			{
				IsOn = true,
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				await ValidatePropertyUpdatesValue(
					view: switchStub,
					property: nameof(ISwitch.IsOn),
					GetPlatformValue: (handler) => handler.PlatformView.On,
					expectedSetValue: false,
					expectedUnsetValue: true);

				var color = OperatingSystem.IsIOSVersionAtLeast(13) ? UIColor.SecondarySystemFill : UIColor.FromRGBA(120, 120, 128, 40);

				await ValidateVisualTrackColor(switchStub, color);
			});
		}

		[Fact(DisplayName = "Apple has not changed UITrack Subviews")]
		public async Task UIViewSubviewExists()
		{
			var switchStub = new SwitchStub();

			await InvokeOnMainThreadAsync(async () =>
			{
				await ValidateTrackSubViewExists(switchStub);
			});
		}

		[Theory(DisplayName = "Custom Colors Use Sliding Style On iOS 26")]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(true, true)]
		public async Task CustomColorsUseSlidingStyleOniOS26(bool hasTrackColor, bool hasThumbColor)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(26))
			{
				return;
			}

			var switchStub = new SwitchStub
			{
				TrackColor = hasTrackColor ? Colors.Red : null,
				ThumbColor = hasThumbColor ? Colors.Orange : null
			};

			var styles = await GetValueAsync(switchStub, handler =>
			{
				var nativeSwitch = GetNativeSwitch(handler);
				return (nativeSwitch.PreferredStyle, nativeSwitch.Style);
			});

			Assert.Equal(UISwitchStyle.Sliding, styles.PreferredStyle);
			Assert.Equal(UISwitchStyle.Sliding, styles.Style);
		}

		[Fact(DisplayName = "Custom Colors Render On Initial Off State On iOS 26")]
		public async Task CustomColorsRenderOnInitialOffStateOniOS26()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(26))
			{
				return;
			}

			var switchStub = new SwitchStub
			{
				IsOn = false,
				TrackColor = Colors.Red,
				ThumbColor = Colors.Orange
			};

			await AttachAndRun(switchStub, async (SwitchHandler handler) =>
			{
				var nativeSwitch = GetNativeSwitch(handler);

				nativeSwitch.SizeToFit();
				nativeSwitch.SetNeedsLayout();
				nativeSwitch.LayoutIfNeeded();

				UIView renderedView = handler.ContainerView is not null ? handler.ContainerView : nativeSwitch;
				renderedView.SetNeedsLayout();
				renderedView.LayoutIfNeeded();

				UIGraphics.BeginImageContextWithOptions(renderedView.Bounds.Size, false, 0);
				renderedView.DrawViewHierarchy(renderedView.Bounds, true);
				var bitmap = UIGraphics.GetImageFromCurrentImageContext();
				UIGraphics.EndImageContext();

				Assert.NotNull(bitmap);
				await bitmap.AssertContainsColor(Colors.Red, tolerance: 0.1);
			});
		}

		[Fact(DisplayName = "Default Switch Uses Automatic Style On iOS 26")]
		public async Task DefaultSwitchUsesAutomaticStyleOniOS26()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(26))
			{
				return;
			}

			var switchStub = new SwitchStub();

			var style = await GetValueAsync(switchStub, handler => GetNativeSwitch(handler).PreferredStyle);

			Assert.Equal(UISwitchStyle.Automatic, style);
		}

		[Fact(DisplayName = "Thumb Color Clears Correctly")]
		public async Task ThumbColorClearsCorrectly()
		{
			var switchStub = new SwitchStub
			{
				ThumbColor = Colors.Red
			};

			var result = await GetValueAsync(switchStub, handler =>
			{
				Assert.NotNull(GetNativeSwitch(handler).ThumbTintColor);
				switchStub.ThumbColor = null;
				handler.UpdateValue(nameof(ISwitch.ThumbColor));
				var nativeSwitch = GetNativeSwitch(handler);
				return (nativeSwitch.ThumbTintColor, nativeSwitch.PreferredStyle);
			});

			Assert.Null(result.ThumbTintColor);

			if (OperatingSystem.IsIOSVersionAtLeast(26))
			{
				Assert.Equal(UISwitchStyle.Automatic, result.PreferredStyle);
			}
		}

	}
}
