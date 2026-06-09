using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;
using AppTheme = Microsoft.Maui.ApplicationModel.AppTheme;
using ControlsApplication = Microsoft.Maui.Controls.Application;
using ControlsSwitch = Microsoft.Maui.Controls.Switch;

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

		UIImage CaptureRenderedSwitch(SwitchHandler handler)
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

			return bitmap;
		}

		async Task AssertSwitchColorsApplied(UISwitch nativeSwitch, Color trackColor, Color thumbColor, string messageSuffix)
		{
			await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.GetTrackColor(), trackColor.ToPlatform(), tolerance: 0.1))
				.AssertEventually(message: $"Native switch track color did not apply before {messageSuffix}.");

			await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.ThumbTintColor, thumbColor.ToPlatform(), tolerance: 0.1))
				.AssertEventually(message: $"Native switch thumb color did not apply before {messageSuffix}.");
		}

		static bool IsIOSOrMacCatalyst26OrNewer()
		{
			return OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26);
		}

		static UIColor GetDefaultOffTrackColor()
		{
			return OperatingSystem.IsIOSVersionAtLeast(13) ? UIColor.SecondarySystemFill : UIColor.FromRGBA(120, 120, 128, 40);
		}

		async Task AssertDefaultSwitchDoesNotReapplyColors(UISwitch nativeSwitch)
		{
			var trackSubview = nativeSwitch.GetTrackSubview();
			Assert.NotNull(trackSubview);

			await Task.Delay(100);
			Assert.Equal(UISwitchStyle.Automatic, nativeSwitch.PreferredStyle);

			var preservedTrackColor = UIColor.FromRGBA(12, 34, 56, 255);
			var preservedOnTintColor = UIColor.FromRGBA(78, 90, 123, 255);
			var preservedThumbColor = UIColor.FromRGBA(45, 67, 89, 255);

			trackSubview.BackgroundColor = preservedTrackColor;
			nativeSwitch.OnTintColor = preservedOnTintColor;
			nativeSwitch.ThumbTintColor = preservedThumbColor;

			nativeSwitch.MovedToWindow();
			await Task.Delay(100);

			Assert.Equal(UISwitchStyle.Automatic, nativeSwitch.PreferredStyle);

			Assert.True(
				ColorComparison.ARGBEquivalent(nativeSwitch.GetTrackColor(), preservedTrackColor, tolerance: 0.1),
				"Default switch track color was unexpectedly reapplied.");

			Assert.True(
				ColorComparison.ARGBEquivalent(nativeSwitch.OnTintColor, preservedOnTintColor, tolerance: 0.1),
				"Default switch on tint color was unexpectedly cleared or reapplied.");

			Assert.True(
				ColorComparison.ARGBEquivalent(nativeSwitch.ThumbTintColor, preservedThumbColor, tolerance: 0.1),
				"Default switch thumb tint color was unexpectedly cleared or reapplied.");
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
			if (IsIOSOrMacCatalyst26OrNewer())
			{
				return;
			}

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

				await ValidateVisualTrackColor(switchStub, GetDefaultOffTrackColor());
			});
		}

		[Fact(DisplayName = "Default Switch Reapplies Legacy Off Track Color Before iOS/MacCatalyst 26")]
		public async Task DefaultSwitchReappliesLegacyOffTrackColorBeforeiOSOrMacCatalyst26()
		{
			if (IsIOSOrMacCatalyst26OrNewer())
			{
				return;
			}

			var switchStub = new SwitchStub
			{
				IsOn = false
			};

			await AttachAndRun(switchStub, async (SwitchHandler handler) =>
			{
				var nativeSwitch = GetNativeSwitch(handler);

				await new Func<bool>(() => nativeSwitch.IsReadyForColorReapply())
					.AssertEventually(message: "Native switch was not ready for legacy color reapply.");

				await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.GetTrackColor(), GetDefaultOffTrackColor(), tolerance: 0.1))
					.AssertEventually(message: "Default switch did not apply the initial legacy off track color.");

				var trackSubview = nativeSwitch.GetTrackSubview();
				Assert.NotNull(trackSubview);

				trackSubview.BackgroundColor = UIColor.Purple;
				Assert.True(
					ColorComparison.ARGBEquivalent(nativeSwitch.GetTrackColor(), UIColor.Purple, tolerance: 0.1),
					"Test setup failed to poison the default switch track color.");

				nativeSwitch.MovedToWindow();

				await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.GetTrackColor(), GetDefaultOffTrackColor(), tolerance: 0.1))
					.AssertEventually(message: "Default switch did not reapply the legacy off track color after moving to a window.");
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

		[Theory(DisplayName = "Custom Colors Use Sliding Style On iOS/MacCatalyst 26")]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(true, true)]
		public async Task CustomColorsUseSlidingStyleOniOSOrMacCatalyst26(bool hasTrackColor, bool hasThumbColor)
		{
			if (!IsIOSOrMacCatalyst26OrNewer())
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

		[Fact(DisplayName = "Custom Colors Render On Initial Off State On iOS/MacCatalyst 26")]
		public async Task CustomColorsRenderOnInitialOffStateOniOSOrMacCatalyst26()
		{
			if (!IsIOSOrMacCatalyst26OrNewer())
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
				await AssertSwitchColorsApplied(nativeSwitch, Colors.Red, Colors.Orange, "initial bitmap capture");

				var bitmap = CaptureRenderedSwitch(handler);
				await bitmap.AssertContainsColor(Colors.Red, tolerance: 0.1);
			});
		}

		[Fact(DisplayName = "Default Switch Uses Automatic Style On iOS/MacCatalyst 26")]
		public async Task DefaultSwitchUsesAutomaticStyleOniOSOrMacCatalyst26()
		{
			if (!IsIOSOrMacCatalyst26OrNewer())
			{
				return;
			}

			var switchStub = new SwitchStub();

			await AttachAndRun(switchStub, async (SwitchHandler handler) =>
			{
				var nativeSwitch = GetNativeSwitch(handler);

				Assert.Equal(UISwitchStyle.Automatic, nativeSwitch.PreferredStyle);
				await AssertDefaultSwitchDoesNotReapplyColors(nativeSwitch);
			});
		}

		[Fact(DisplayName = "Thumb Color Clears When Reset On iOS/MacCatalyst 26")]
		public async Task ThumbColorClearsWhenResetOniOSOrMacCatalyst26()
		{
			if (!IsIOSOrMacCatalyst26OrNewer())
			{
				return;
			}

			var switchStub = new SwitchStub
			{
				TrackColor = Colors.Red,
				ThumbColor = Colors.Orange
			};

			await AttachAndRun(switchStub, async (SwitchHandler handler) =>
			{
				var nativeSwitch = GetNativeSwitch(handler);

				await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.ThumbTintColor, Colors.Orange.ToPlatform(), tolerance: 0.1))
					.AssertEventually(message: "Native switch thumb color did not update to the custom color.");

				switchStub.ThumbColor = null;
				handler.UpdateValue(nameof(ISwitch.ThumbColor));

				Assert.Equal(UISwitchStyle.Sliding, nativeSwitch.PreferredStyle);
				Assert.Null(nativeSwitch.ThumbTintColor);
			});
		}

		[Fact(DisplayName = "Custom Colors Clear To Automatic Style On iOS/MacCatalyst 26")]
		public async Task CustomColorsClearToAutomaticStyleOniOSOrMacCatalyst26()
		{
			if (!IsIOSOrMacCatalyst26OrNewer())
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

				await AssertSwitchColorsApplied(nativeSwitch, Colors.Red, Colors.Orange, "resetting custom colors");

				switchStub.TrackColor = null;
				switchStub.ThumbColor = null;
				handler.UpdateValue(nameof(ISwitch.TrackColor));
				handler.UpdateValue(nameof(ISwitch.ThumbColor));

				Assert.Equal(UISwitchStyle.Automatic, nativeSwitch.PreferredStyle);
				Assert.Null(nativeSwitch.OnTintColor);
				Assert.Null(nativeSwitch.ThumbTintColor);
				Assert.False(
					ColorComparison.ARGBEquivalent(nativeSwitch.GetTrackColor(), Colors.Red.ToPlatform(), tolerance: 0.1),
					"Native switch track color kept the stale custom color after custom colors were cleared.");

				await AssertDefaultSwitchDoesNotReapplyColors(nativeSwitch);
			});
		}

		[Fact(DisplayName = "Custom Colors Reapply After Moved To Window On iOS/MacCatalyst 26")]
		public async Task CustomColorsReapplyAfterMovedToWindowOniOSOrMacCatalyst26()
		{
			if (!IsIOSOrMacCatalyst26OrNewer())
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

				await new Func<bool>(() => nativeSwitch.IsReadyForColorReapply())
					.AssertEventually(message: "Native switch was not ready for color reapply.");

				await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.GetTrackColor(), Colors.Red.ToPlatform(), tolerance: 0.1))
					.AssertEventually(message: "Native switch track color did not initially apply.");

				await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.ThumbTintColor, Colors.Orange.ToPlatform(), tolerance: 0.1))
					.AssertEventually(message: "Native switch thumb color did not initially apply.");

				var trackSubview = nativeSwitch.GetTrackSubview();
				Assert.NotNull(trackSubview);

				trackSubview.BackgroundColor = UIColor.Clear;
				nativeSwitch.ThumbTintColor = UIColor.Purple;

				nativeSwitch.MovedToWindow();

				await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.GetTrackColor(), Colors.Red.ToPlatform(), tolerance: 0.1))
					.AssertEventually(message: "Native switch track color did not reapply after moving to a window.");

				await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.ThumbTintColor, Colors.Orange.ToPlatform(), tolerance: 0.1))
					.AssertEventually(message: "Native switch thumb color did not reapply after moving to a window.");
			});
		}

		[Fact(DisplayName = "Lifecycle Color Reapply Uses Mapper Customizations On iOS/MacCatalyst 26")]
		public async Task LifecycleColorReapplyUsesMapperCustomizationsOniOSOrMacCatalyst26()
		{
			if (!IsIOSOrMacCatalyst26OrNewer())
			{
				return;
			}

			var mapperTrackColor = UIColor.Purple;
			var mapperRanOffMainThread = false;
			var switchStub = new SwitchStub
			{
				IsOn = false,
				TrackColor = Colors.Red,
				ThumbColor = Colors.Orange
			};

			CustomSwitchHandler.TestMapper = new PropertyMapper<ISwitch, ISwitchHandler>(SwitchHandler.Mapper);
			CustomSwitchHandler.TestMapper.AppendToMapping(nameof(ISwitch.TrackColor), (handler, view) =>
			{
				mapperRanOffMainThread |= !NSThread.IsMain;

				if (handler.PlatformView is UISwitch uiSwitch && uiSwitch.GetTrackSubview() is UIView trackSubview)
				{
					trackSubview.BackgroundColor = mapperTrackColor;
				}
			});

			try
			{
				await AttachAndRun<CustomSwitchHandler>(switchStub, async (CustomSwitchHandler handler) =>
				{
					var nativeSwitch = GetNativeSwitch(handler);

					await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.GetTrackColor(), mapperTrackColor, tolerance: 0.1))
						.AssertEventually(message: "Custom TrackColor mapper did not run during initial mapping.");

					Assert.False(mapperRanOffMainThread, "Custom switch color mapper ran off the main thread.");

					var trackSubview = nativeSwitch.GetTrackSubview();
					Assert.NotNull(trackSubview);

					trackSubview.BackgroundColor = UIColor.Clear;
					nativeSwitch.MovedToWindow();

					await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.GetTrackColor(), mapperTrackColor, tolerance: 0.1))
						.AssertEventually(message: "Lifecycle color reapply bypassed the custom TrackColor mapper.");

					Assert.False(mapperRanOffMainThread, "Lifecycle color reapply ran the custom mapper off the main thread.");
				});
			}
			finally
			{
				CustomSwitchHandler.TestMapper = new PropertyMapper<ISwitch, ISwitchHandler>(SwitchHandler.Mapper);
			}
		}

		[Fact(DisplayName = "Mapper Only Color Reapply Uses Sliding Style On iOS/MacCatalyst 26")]
		public async Task MapperOnlyColorReapplyUsesSlidingStyleOniOSOrMacCatalyst26()
		{
			if (!IsIOSOrMacCatalyst26OrNewer())
			{
				return;
			}

			var mapperTrackColor = UIColor.Purple;
			var mapperRanOffMainThread = false;
			var switchStub = new SwitchStub
			{
				IsOn = false
			};

			CustomSwitchHandler.TestMapper = new PropertyMapper<ISwitch, ISwitchHandler>(SwitchHandler.Mapper);
			CustomSwitchHandler.TestMapper.AppendToMapping(nameof(ISwitch.TrackColor), (handler, view) =>
			{
				mapperRanOffMainThread |= !NSThread.IsMain;

				if (handler.PlatformView is UISwitch uiSwitch && uiSwitch.GetTrackSubview() is UIView trackSubview)
				{
					trackSubview.BackgroundColor = mapperTrackColor;
				}
			});

			try
			{
				await AttachAndRun<CustomSwitchHandler>(switchStub, async (CustomSwitchHandler handler) =>
				{
					var nativeSwitch = GetNativeSwitch(handler);

					await new Func<bool>(() => nativeSwitch.IsReadyForColorReapply())
						.AssertEventually(message: "Native switch was not ready for mapper-only color reapply.");

					await new Func<bool>(() => nativeSwitch.PreferredStyle == UISwitchStyle.Sliding && nativeSwitch.Style == UISwitchStyle.Sliding)
						.AssertEventually(message: "Mapper-only switch color customization did not opt into Sliding style.");

					await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.GetTrackColor(), mapperTrackColor, tolerance: 0.1))
						.AssertEventually(message: "Mapper-only TrackColor customization did not apply during initial color reapply.");

					Assert.Null(switchStub.TrackColor);
					Assert.Null(switchStub.ThumbColor);
					Assert.False(mapperRanOffMainThread, "Mapper-only switch color mapper ran off the main thread.");

					var trackSubview = nativeSwitch.GetTrackSubview();
					Assert.NotNull(trackSubview);

					trackSubview.BackgroundColor = UIColor.Clear;
					nativeSwitch.MovedToWindow();

					await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.GetTrackColor(), mapperTrackColor, tolerance: 0.1))
						.AssertEventually(message: "Lifecycle color reapply skipped the mapper-only TrackColor customization.");

					Assert.False(mapperRanOffMainThread, "Mapper-only lifecycle color reapply ran off the main thread.");
				});
			}
			finally
			{
				CustomSwitchHandler.TestMapper = new PropertyMapper<ISwitch, ISwitchHandler>(SwitchHandler.Mapper);
			}
		}

		[Fact(DisplayName = "Layout Subviews Does Not Reapply Colors When Not Dirty On iOS/MacCatalyst 26")]
		public async Task LayoutSubviewsDoesNotReapplyColorsWhenNotDirtyOniOSOrMacCatalyst26()
		{
			if (!IsIOSOrMacCatalyst26OrNewer())
			{
				return;
			}

			var trackMapperCalls = 0;
			var thumbMapperCalls = 0;
			var mapperRanOffMainThread = false;
			var switchStub = new SwitchStub
			{
				IsOn = false,
				TrackColor = Colors.Red,
				ThumbColor = Colors.Orange
			};

			CustomSwitchHandler.TestMapper = new PropertyMapper<ISwitch, ISwitchHandler>(SwitchHandler.Mapper);
			CustomSwitchHandler.TestMapper.AppendToMapping(nameof(ISwitch.TrackColor), (_, _) =>
			{
				trackMapperCalls++;
				mapperRanOffMainThread |= !NSThread.IsMain;
			});
			CustomSwitchHandler.TestMapper.AppendToMapping(nameof(ISwitch.ThumbColor), (_, _) =>
			{
				thumbMapperCalls++;
				mapperRanOffMainThread |= !NSThread.IsMain;
			});

			try
			{
				await AttachAndRun<CustomSwitchHandler>(switchStub, async (CustomSwitchHandler handler) =>
				{
					var nativeSwitch = GetNativeSwitch(handler);

					await AssertSwitchColorsApplied(nativeSwitch, Colors.Red, Colors.Orange, "initial mapper-count setup");

					await new Func<bool>(() => trackMapperCalls >= 2 && thumbMapperCalls >= 2)
						.AssertEventually(message: "Initial dirty color reapply did not run through the mapper.");

					Assert.False(mapperRanOffMainThread, "Initial switch color mapper ran off the main thread.");

					var trackMapperCallsAfterInitialReapply = trackMapperCalls;
					var thumbMapperCallsAfterInitialReapply = thumbMapperCalls;

					nativeSwitch.LayoutSubviews();
					nativeSwitch.LayoutSubviews();
					nativeSwitch.LayoutSubviews();

					await Task.Delay(100);

					Assert.Equal(trackMapperCallsAfterInitialReapply, trackMapperCalls);
					Assert.Equal(thumbMapperCallsAfterInitialReapply, thumbMapperCalls);
					Assert.False(mapperRanOffMainThread, "LayoutSubviews color mapper ran off the main thread.");
				});
			}
			finally
			{
				CustomSwitchHandler.TestMapper = new PropertyMapper<ISwitch, ISwitchHandler>(SwitchHandler.Mapper);
			}
		}

		[Fact(DisplayName = "Queued Color Reapply Does Not Update Stale Native Switch After Reconnect On iOS/MacCatalyst 26")]
		public async Task QueuedColorReapplyDoesNotUpdateStaleNativeSwitchAfterReconnectOniOSOrMacCatalyst26()
		{
			if (!IsIOSOrMacCatalyst26OrNewer())
			{
				return;
			}

			var switchStub = new SwitchStub
			{
				IsOn = false,
				TrackColor = Colors.Red,
				ThumbColor = Colors.Orange
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(switchStub);
				var staleNativeSwitch = GetNativeSwitch(handler);
				var staleTrackColor = UIColor.FromRGBA(12, 34, 56, 255);
				var staleThumbColor = UIColor.FromRGBA(45, 67, 89, 255);

				staleNativeSwitch.SizeToFit();
				staleNativeSwitch.SetNeedsLayout();
				staleNativeSwitch.LayoutIfNeeded();

				var staleTrackSubview = staleNativeSwitch.GetTrackSubview();
				Assert.NotNull(staleTrackSubview);

				staleTrackSubview.BackgroundColor = staleTrackColor;
				staleNativeSwitch.ThumbTintColor = staleThumbColor;

				((IElementHandler)handler).DisconnectHandler();
				InitializeViewHandler(switchStub, handler);

				var currentNativeSwitch = GetNativeSwitch(handler);
				Assert.NotSame(staleNativeSwitch, currentNativeSwitch);

				await Task.Delay(100);

				Assert.True(
					ColorComparison.ARGBEquivalent(staleNativeSwitch.GetTrackColor(), staleTrackColor, tolerance: 0.1),
					"Queued color reapply updated the stale native switch track color after reconnect.");

				Assert.True(
					ColorComparison.ARGBEquivalent(staleNativeSwitch.ThumbTintColor, staleThumbColor, tolerance: 0.1),
					"Queued color reapply updated the stale native switch thumb color after reconnect.");

				await AssertSwitchColorsApplied(currentNativeSwitch, Colors.Red, Colors.Orange, "reconnect");
			});
		}

#if IOS
		[Fact(DisplayName = "Custom Colors Reapply After Will Enter Foreground On iOS 26")]
		public async Task CustomColorsReapplyAfterWillEnterForegroundOniOS26()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(26))
			{
				return;
			}

			var switchStub = new SwitchStub
			{
				IsOn = true,
				TrackColor = Colors.Red,
				ThumbColor = Colors.Orange
			};

			await AttachAndRun(switchStub, async (SwitchHandler handler) =>
			{
				var nativeSwitch = GetNativeSwitch(handler);

				await AssertSwitchColorsApplied(nativeSwitch, Colors.Red, Colors.Orange, "foreground reapply setup");

				var trackSubview = nativeSwitch.GetTrackSubview();
				Assert.NotNull(trackSubview);

				trackSubview.BackgroundColor = UIColor.Clear;
				nativeSwitch.OnTintColor = UIColor.Clear;
				nativeSwitch.ThumbTintColor = UIColor.Purple;

				NSNotificationCenter.DefaultCenter.PostNotificationName(
					UIApplication.WillEnterForegroundNotification,
					UIApplication.SharedApplication);

				await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.OnTintColor, Colors.Red.ToPlatform(), tolerance: 0.1))
					.AssertEventually(message: "Native switch on tint color did not reapply after foregrounding.");

				await AssertSwitchColorsApplied(nativeSwitch, Colors.Red, Colors.Orange, "foregrounding");
			});
		}

		[Fact(DisplayName = "Mapper Only Colors Reapply After Will Enter Foreground On iOS 26")]
		public async Task MapperOnlyColorsReapplyAfterWillEnterForegroundOniOS26()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(26))
			{
				return;
			}

			var mapperTrackColor = UIColor.Purple;
			var mapperRanOffMainThread = false;
			var switchStub = new SwitchStub
			{
				IsOn = true
			};

			CustomSwitchHandler.TestMapper = new PropertyMapper<ISwitch, ISwitchHandler>(SwitchHandler.Mapper);
			CustomSwitchHandler.TestMapper.AppendToMapping(nameof(ISwitch.TrackColor), (handler, view) =>
			{
				mapperRanOffMainThread |= !NSThread.IsMain;

				if (handler.PlatformView is UISwitch uiSwitch)
				{
					uiSwitch.OnTintColor = mapperTrackColor;

					if (uiSwitch.GetTrackSubview() is UIView trackSubview)
					{
						trackSubview.BackgroundColor = mapperTrackColor;
					}
				}
			});

			try
			{
				await AttachAndRun<CustomSwitchHandler>(switchStub, async (CustomSwitchHandler handler) =>
				{
					var nativeSwitch = GetNativeSwitch(handler);

					await new Func<bool>(() => nativeSwitch.PreferredStyle == UISwitchStyle.Sliding && nativeSwitch.Style == UISwitchStyle.Sliding)
						.AssertEventually(message: "Mapper-only foreground test did not opt into Sliding style.");

					await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.OnTintColor, mapperTrackColor, tolerance: 0.1))
						.AssertEventually(message: "Mapper-only OnTintColor did not apply during initial color reapply.");

					Assert.Null(switchStub.TrackColor);
					Assert.Null(switchStub.ThumbColor);
					Assert.False(mapperRanOffMainThread, "Mapper-only switch color mapper ran off the main thread.");

					var trackSubview = nativeSwitch.GetTrackSubview();
					Assert.NotNull(trackSubview);

					trackSubview.BackgroundColor = UIColor.Clear;
					nativeSwitch.OnTintColor = UIColor.Clear;

					NSNotificationCenter.DefaultCenter.PostNotificationName(
						UIApplication.WillEnterForegroundNotification,
						UIApplication.SharedApplication);

					await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.OnTintColor, mapperTrackColor, tolerance: 0.1))
						.AssertEventually(message: "Mapper-only OnTintColor did not reapply after foregrounding.");

					await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.GetTrackColor(), mapperTrackColor, tolerance: 0.1))
						.AssertEventually(message: "Mapper-only track color did not reapply after foregrounding.");

					Assert.False(mapperRanOffMainThread, "Mapper-only foreground color reapply ran off the main thread.");
				});
			}
			finally
			{
				CustomSwitchHandler.TestMapper = new PropertyMapper<ISwitch, ISwitchHandler>(SwitchHandler.Mapper);
			}
		}
#endif

		[Fact(DisplayName = "Custom Colors Update After App Theme Change On iOS/MacCatalyst 26")]
		public async Task CustomColorsUpdateAfterAppThemeChangeOniOSOrMacCatalyst26()
		{
			if (!IsIOSOrMacCatalyst26OrNewer())
			{
				return;
			}

			var previousApplication = ControlsApplication.Current;
			var previousTheme = previousApplication?.UserAppTheme ?? AppTheme.Unspecified;
			var application = new ControlsApplication();
			var switchView = new ControlsSwitch
			{
				IsToggled = false
			};

			application.UserAppTheme = AppTheme.Light;
#pragma warning disable CS0618 // MainPage is enough to parent the test element for app-theme resource propagation.
			application.MainPage = new ContentPage { Content = switchView };
#pragma warning restore CS0618
			switchView.SetAppThemeColor(ControlsSwitch.OffColorProperty, Colors.Red, Colors.Blue);
			switchView.SetAppThemeColor(ControlsSwitch.ThumbColorProperty, Colors.Orange, Colors.Yellow);
			application.UpdateUserInterfaceStyle();

			try
			{
				await AttachAndRun(switchView, async (SwitchHandler handler) =>
				{
					var nativeSwitch = GetNativeSwitch(handler);

					await AssertSwitchColorsApplied(nativeSwitch, Colors.Red, Colors.Orange, "initial theme bitmap capture");
					await CaptureRenderedSwitch(handler).AssertContainsColor(Colors.Red, tolerance: 0.1);

					application.UserAppTheme = AppTheme.Dark;
					application.UpdateUserInterfaceStyle();

					await new Func<bool>(() => switchView.OffColor == Colors.Blue)
						.AssertEventually(message: "Switch OffColor did not update to the dark app theme color.");

					await new Func<bool>(() => switchView.ThumbColor == Colors.Yellow)
						.AssertEventually(message: "Switch ThumbColor did not update to the dark app theme color.");

					await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.GetTrackColor(), Colors.Blue.ToPlatform(), tolerance: 0.1))
						.AssertEventually(message: "Native switch track color did not update to the dark app theme color.");

					await new Func<bool>(() => ColorComparison.ARGBEquivalent(nativeSwitch.ThumbTintColor, Colors.Yellow.ToPlatform(), tolerance: 0.1))
						.AssertEventually(message: "Native switch thumb color did not update to the dark app theme color.");

					await new Func<Task<bool>>(async () =>
					{
						try
						{
							await CaptureRenderedSwitch(handler).AssertContainsColor(Colors.Blue, tolerance: 0.1);
							return true;
						}
						catch
						{
							return false;
						}
					}).AssertEventuallyAsync(message: "Rendered switch track did not update to the dark app theme color.");
				});
			}
			finally
			{
				application.UserAppTheme = AppTheme.Unspecified;
				application.UpdateUserInterfaceStyle();
				ControlsApplication.Current = previousApplication;

				if (previousApplication is not null)
				{
					previousApplication.UserAppTheme = previousTheme;
					previousApplication.UpdateUserInterfaceStyle();
				}
			}
		}

	}

	class CustomSwitchHandler : SwitchHandler
	{
		public static PropertyMapper<ISwitch, ISwitchHandler> TestMapper = new(Mapper);

		public CustomSwitchHandler()
			: base(TestMapper, CommandMapper)
		{
		}
	}
}
