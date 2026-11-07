#if IOS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class ShellTabSwitchTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task CustomItemRendererBackgroundIsPreservedForTransparentPages()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(18) && !OperatingSystem.IsMacCatalystVersionAtLeast(18))
				return;

			await InvokeOnMainThreadAsync(() =>
			{
				using var view = new UIView();
				using var initialBackground = Colors.Gray.ToPlatform();
				using var rendererBackground = Colors.Red.ToPlatform();
				view.BackgroundColor = rendererBackground;

				var backgroundState = new ShellItemRenderer.ContentBackgroundState
				{
					DefaultBackgroundColor = initialBackground
				};
				var transparentPage = new ContentPage();
				var opaquePage = new ContentPage { Background = new SolidPaint(Colors.Blue) };

				ShellItemRenderer.UpdateContentBackground(view, transparentPage, backgroundState);
				Assert.True(ColorComparison.ARGBEquivalent(rendererBackground, view.BackgroundColor));

				ShellItemRenderer.UpdateContentBackground(view, opaquePage, backgroundState);
				Assert.True(backgroundState.OverrideApplied);

				ShellItemRenderer.UpdateContentBackground(view, transparentPage, backgroundState);
				Assert.False(backgroundState.OverrideApplied);
				Assert.True(ColorComparison.ARGBEquivalent(rendererBackground, view.BackgroundColor));

				ShellItemRenderer.UpdateContentBackground(view, opaquePage, backgroundState);
				view.BackgroundColor = rendererBackground;
				ShellItemRenderer.UpdateContentBackground(view, transparentPage, backgroundState);
				Assert.True(ColorComparison.ARGBEquivalent(rendererBackground, view.BackgroundColor));
			});
		}

		[Theory(DisplayName = "Displayed page background changes restore the captured native color")]
		[InlineData(true, false)]
		[InlineData(true, true)]
		[InlineData(false, false)]
		[InlineData(false, true)]
		public async Task DisplayedPageBackgroundChangesRestoreCapturedNativeColor(bool useLegacyRenderer, bool excludeByOpacity)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(18))
				return;

			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
			{
				SetupShellHandlers(handlers);
				handlers.AddHandler(typeof(Shell), useLegacyRenderer ? typeof(ShellRenderer) : typeof(ShellHandler));
				if (!useLegacyRenderer)
				{
					handlers.AddHandler<ShellItem, ShellItemHandler>();
					handlers.AddHandler<ShellSection, ShellSectionHandler>();
					handlers.AddHandler<ShellContent, ShellContentHandler>();
				}
			}));

			var page = new ContentPage
			{
				Background = new SolidPaint(Colors.Transparent),
				Content = new Grid { BackgroundColor = Colors.Black }
			};
			var shell = new Shell
			{
				FlyoutBehavior = FlyoutBehavior.Disabled,
				Items =
				{
					new TabBar
					{
						Items =
						{
							new Tab { Title = "Current", Items = { new ShellContent { Content = page } } },
							new Tab { Title = "Other", Items = { new ShellContent { Content = new ContentPage() } } }
						}
					}
				}
			};

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await Task.Delay(500);
				var platformPage = ((IPlatformViewHandler)page.Handler).PlatformView;
				var container = platformPage.FindParent(view => view.NextResponder is UITabBarController);
				var controller = Assert.IsAssignableFrom<UITabBarController>(container?.NextResponder);
				Assert.Same(page, shell.CurrentPage);

				using var capturedDefault = Colors.Red.ToPlatform();
				controller.View.BackgroundColor = capturedDefault;

				page.Background = new SolidPaint(Colors.Black);
				Assert.False(ColorComparison.ARGBEquivalent(capturedDefault, controller.View.BackgroundColor),
					$"The opaque page should replace the captured backing color (legacy={useLegacyRenderer}).");

				if (excludeByOpacity)
					page.Opacity = 0.5;
				else
					page.Background = new SolidPaint(Colors.Transparent);

				Assert.True(ColorComparison.ARGBEquivalent(capturedDefault, controller.View.BackgroundColor),
					$"Excluding the page background should restore the captured red backing color (legacy={useLegacyRenderer}, opacity={excludeByOpacity}).");

				if (excludeByOpacity)
					page.Opacity = 1;
				else
					page.Background = new SolidPaint(Colors.Black);

				Assert.False(ColorComparison.ARGBEquivalent(capturedDefault, controller.View.BackgroundColor),
					$"Restoring an opaque page should replace the captured backing color again (legacy={useLegacyRenderer}, opacity={excludeByOpacity}).");
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		[SkipOnIOSVersion(26, "Liquid Glass tab bars no longer expose UIControl tab buttons for native TouchUpInside actions (see #33004).")]
		public async Task BlackBackgroundRemainsBlackDuringNativeTabSwitches(bool useLegacyRenderer)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(18))
				return;

			EnsureHandlerCreated(builder => builder.ConfigureMauiHandlers(handlers =>
			{
				SetupShellHandlers(handlers);
				handlers.AddHandler(typeof(Shell), useLegacyRenderer ? typeof(ShellRenderer) : typeof(ShellHandler));
				if (!useLegacyRenderer)
				{
					handlers.AddHandler<ShellItem, ShellItemHandler>();
					handlers.AddHandler<ShellSection, ShellSectionHandler>();
					handlers.AddHandler<ShellContent, ShellContentHandler>();
				}
			}));

			var tabs = new TabBar();
			foreach (var title in new[] { "Home", "Draws", "Investments", "Holders" })
			{
				var page = new ContentPage
				{
					BackgroundColor = Colors.Black,
					Content = new Grid
					{
						BackgroundColor = Colors.Black,
						Children =
						{
							new Label
							{
								Text = title, TextColor = Colors.White,
								HorizontalOptions = LayoutOptions.Center,
								VerticalOptions = LayoutOptions.Center
							}
						}
					}
				};
				Shell.SetNavBarIsVisible(page, false);
				tabs.Items.Add(new Tab { Title = title, Items = { new ShellContent { Content = page } } });
			}

			var shell = new Shell { FlyoutBehavior = FlyoutBehavior.Disabled, BackgroundColor = Colors.Black, Items = { tabs } };
			Shell.SetTabBarBackgroundColor(shell, Colors.Black);
			Shell.SetTabBarForegroundColor(shell, Colors.White);
			Shell.SetTabBarUnselectedColor(shell, Colors.White);

			await CreateHandlerAndAddToWindow(shell, async () =>
			{
				await Task.Delay(500);
				var platformPage = ((IPlatformViewHandler)shell.CurrentPage.Handler).PlatformView;
				var container = platformPage.FindParent(view => view.NextResponder is UITabBarController);
				var controller = Assert.IsAssignableFrom<UITabBarController>(container?.NextResponder);
				var window = controller.View.Window;
				Assert.NotNull(window);
				var buttons = GetTabButtons(controller.TabBar)
					.OrderBy(button => button.ConvertRectToView(button.Bounds, controller.TabBar).X).ToArray();
				Assert.Equal(4, buttons.Length);
				Assert.Equal(0, (int)controller.SelectedIndex);

				// Sample an empty area relative to the page, away from the centered label,
				// safe areas and tab bar. Keep this window coordinate fixed during transitions.
				var pageView = controller.SelectedViewController.View;
				var safeFrame = pageView.SafeAreaLayoutGuide.LayoutFrame;
				Assert.True(safeFrame.Width > 0 && safeFrame.Height > 0);
				var sample = pageView.ConvertPointToView(
					new CGPoint(safeFrame.X + safeFrame.Width / 4, safeFrame.Y + safeFrame.Height / 4), window);
				Assert.InRange(ReadBackgroundBrightness(window, sample), 0, 8);

				int peak = 0;
				int frames = 0;
				var selectedIndices = new List<int> { 0 };
				var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
				double nextSwitch = 0;
				using var displayLink = CADisplayLink.Create(() =>
				{
					// Never throw assertions/exceptions through a native display-link callback.
					try
					{
						peak = Math.Max(peak, ReadBackgroundBrightness(window, sample));
						frames++;
						var now = CAAnimation.CurrentMediaTime();
						if (now < nextSwitch)
							return;
						if (selectedIndices.Count == 9)
						{
							completed.TrySetResult();
							return;
						}

						int index = selectedIndices.Count % buttons.Length;
						// Exercise UIKit's actual tab action, not a managed CurrentItem assignment.
						buttons[index].SendActionForControlEvents(UIControlEvent.TouchUpInside);
						selectedIndices.Add((int)controller.SelectedIndex);
						nextSwitch = now + 0.75;
					}
					catch (Exception ex)
					{
						completed.TrySetException(ex);
					}
				});
				displayLink.AddToRunLoop(NSRunLoop.Main, NSRunLoopMode.Common);
				try
				{
					await completed.Task.WaitAsync(TimeSpan.FromSeconds(12));
				}
				finally
				{
					displayLink.Invalidate();
				}

				Assert.Equal(new[] { 0, 1, 2, 3, 0, 1, 2, 3, 0 }, selectedIndices);
				Assert.True(frames >= 16, $"Only sampled {frames} frames.");
				Assert.True(peak <= 8,
					$"Issue39800: background flashed during native tab switching (legacy={useLegacyRenderer}, peak brightness={peak}/255, sampled frames={frames}).");
			});
		}

		static IEnumerable<UIControl> GetTabButtons(UIView view)
		{
			foreach (var child in view.Subviews)
			{
				if (child is UIControl control && (control.AllControlEvents & UIControlEvent.TouchUpInside) != 0)
					yield return control;
				else
					foreach (var button in GetTabButtons(child))
						yield return button;
			}
		}

		static int ReadBackgroundBrightness(UIWindow window, CGPoint sample)
		{
			using var format = new UIGraphicsImageRendererFormat { Scale = 1, Opaque = true };
			using var renderer = new UIGraphicsImageRenderer(window.Bounds.Size, format);
			bool drawn = false;
			using var image = renderer.CreateImage(_ => drawn = window.DrawViewHierarchy(window.Bounds, false));
			if (!drawn)
				throw new InvalidOperationException("Could not capture the window hierarchy.");

			using var cgImage = image.CGImage;
			using var pixel = cgImage.WithImageInRect(new CGRect((int)sample.X, (int)sample.Y, 1, 1));
			using var colorSpace = CGColorSpace.CreateDeviceRGB();
			// Normalize renderer-dependent channel ordering/alpha to RGBA before reading.
			using var context = new CGBitmapContext(IntPtr.Zero, 1, 1, 8, 4, colorSpace,
				CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast);
			context.DrawImage(new CGRect(0, 0, 1, 1), pixel);
			if (Marshal.ReadByte(context.Data, 3) != 255)
				throw new InvalidOperationException("Background sample must be opaque.");
			return Math.Max(Marshal.ReadByte(context.Data), Math.Max(
				Marshal.ReadByte(context.Data, 1), Marshal.ReadByte(context.Data, 2)));
		}
	}
}
#endif
