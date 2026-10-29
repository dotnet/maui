using System;
using System.Linq;
using System.Threading.Tasks;
using CoreAnimation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public class SafeShellTabBarAppearanceTrackerTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(false, false, false)]
		[InlineData(false, true, false)]
		[InlineData(true, false, false)]
		[InlineData(true, true, false)]
		[InlineData(false, false, true)]
		[InlineData(false, true, true)]
		[InlineData(true, false, true)]
		[InlineData(true, true, true)]
		public async Task LegacyBackgroundColorPreservesNativeViewBackground(bool tabBarColor, bool afterGradient, bool hasNativeBackground)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15))
				return;

			await InvokeOnMainThreadAsync(() =>
			{
				using var tabBar = new UITabBar();
				var nativeBackground = hasNativeBackground ? Colors.Yellow.ToPlatform() : null;
				tabBar.BackgroundColor = nativeBackground;
				using var controller = new TestTabBarController(tabBar);
				using var tracker = new SafeShellTabBarAppearanceTracker();
				var shellItem = new ShellItem();
				if (afterGradient)
				{
					Shell.SetBackground(shellItem, new LinearGradientBrush(
						new GradientStopCollection
						{
							new GradientStop(Colors.Red, 0),
							new GradientStop(Colors.Blue, 1)
						},
						new Point(0, 0),
						new Point(1, 1)));
					var gradientAppearance = new ShellAppearance();
					gradientAppearance.Ingest(shellItem);
					tracker.SetAppearance(controller, gradientAppearance);
					shellItem.ClearValue(Shell.BackgroundProperty);
				}

				if (tabBarColor)
					Shell.SetTabBarBackgroundColor(shellItem, Colors.Blue);
				else
					Shell.SetBackgroundColor(shellItem, Colors.Blue);
				var appearance = new ShellAppearance();
				appearance.Ingest(shellItem);

				tracker.SetAppearance(controller, appearance);

				Assert.True(ColorComparison.ARGBEquivalent(Colors.Blue.ToPlatform(), tabBar.StandardAppearance.BackgroundColor));
				Assert.True(ColorComparison.ARGBEquivalent(nativeBackground, tabBar.BackgroundColor));
				Assert.DoesNotContain(tabBar.Layer.Sublayers ?? Array.Empty<CALayer>(), layer => layer is CAGradientLayer);
			});
		}

		[Fact]
		public async Task ResetRestoresOriginalNativeAppearances()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15))
				return;

			await InvokeOnMainThreadAsync(() =>
			{
				using var tabBar = new UITabBar();
				using var controller = new TestTabBarController(tabBar);
				using var standard = new UITabBarAppearance();
				using var scrollEdge = new UITabBarAppearance();
				standard.ConfigureWithOpaqueBackground();
				standard.BackgroundColor = Colors.Purple.ToPlatform();
				scrollEdge.ConfigureWithTransparentBackground();
				scrollEdge.BackgroundColor = Colors.Green.ToPlatform();
				tabBar.StandardAppearance = standard;
				tabBar.ScrollEdgeAppearance = scrollEdge;
				tabBar.BackgroundColor = Colors.Yellow.ToPlatform();
				using var tracker = new SafeShellTabBarAppearanceTracker();
				var shellItem = new ShellItem();
				Shell.SetBackground(shellItem, new SolidColorBrush(Colors.Blue));
				var appearance = new ShellAppearance();
				appearance.Ingest(shellItem);

				tracker.SetAppearance(controller, appearance);
				tracker.ResetAppearance(controller);

				Assert.True(ColorComparison.ARGBEquivalent(Colors.Purple.ToPlatform(), tabBar.StandardAppearance.BackgroundColor));
				Assert.True(ColorComparison.ARGBEquivalent(Colors.Green.ToPlatform(), tabBar.ScrollEdgeAppearance.BackgroundColor));
				Assert.True(ColorComparison.ARGBEquivalent(Colors.Yellow.ToPlatform(), tabBar.BackgroundColor));
				Assert.Null(tabBar.StandardAppearance.BackgroundEffect);
				Assert.Null(tabBar.ScrollEdgeAppearance.BackgroundEffect);
			});
		}

		[Fact]
		public async Task ResetRestoresStandardMaterialWhenOriginalScrollEdgeAppearanceIsNull()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15))
				return;

			await InvokeOnMainThreadAsync(() =>
			{
				using var tabBar = new UITabBar();
				using var controller = new TestTabBarController(tabBar);
				using var standard = new UITabBarAppearance();
				standard.ConfigureWithDefaultBackground();
				standard.BackgroundColor = Colors.Purple.ToPlatform();
				tabBar.StandardAppearance = standard;
				tabBar.ScrollEdgeAppearance = null;
				using var tracker = new SafeShellTabBarAppearanceTracker();
				var shellItem = new ShellItem();
				Shell.SetBackground(shellItem, new LinearGradientBrush(
					new GradientStopCollection
					{
						new GradientStop(Colors.Red, 0),
						new GradientStop(Colors.Blue, 1)
					},
					new Point(0, 0),
					new Point(1, 1)));
				var gradientAppearance = new ShellAppearance();
				gradientAppearance.Ingest(shellItem);
				tracker.SetAppearance(controller, gradientAppearance);

				Shell.SetBackground(shellItem, new SolidColorBrush(Colors.Blue));
				var solidAppearance = new ShellAppearance();
				solidAppearance.Ingest(shellItem);
				tracker.SetAppearance(controller, solidAppearance);
				tracker.ResetAppearance(controller);

				Assert.NotNull(tabBar.ScrollEdgeAppearance);
				Assert.True(ColorComparison.ARGBEquivalent(Colors.Purple.ToPlatform(), tabBar.StandardAppearance.BackgroundColor));
				Assert.True(ColorComparison.ARGBEquivalent(Colors.Purple.ToPlatform(), tabBar.ScrollEdgeAppearance.BackgroundColor));
				Assert.NotNull(tabBar.StandardAppearance.BackgroundEffect);
				Assert.NotNull(tabBar.ScrollEdgeAppearance.BackgroundEffect);
				Assert.DoesNotContain(tabBar.Layer.Sublayers ?? Array.Empty<CALayer>(), layer => layer is CAGradientLayer);
			});
		}

		[Fact]
		public async Task ResetRestoresCapturedAppearanceWhenOriginalTintIsNull()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15))
				return;

			await InvokeOnMainThreadAsync(() =>
			{
				using var tabBar = new NullTintTabBar();
				using var controller = new TestTabBarController(tabBar);
				using var tracker = new SafeShellTabBarAppearanceTracker();
				tabBar.BarTintColor = Colors.Purple.ToPlatform();
				tabBar.UnselectedItemTintColor = Colors.Green.ToPlatform();
				var shellItem = new ShellItem();
				Shell.SetBackground(shellItem, new LinearGradientBrush(
					new GradientStopCollection
					{
						new GradientStop(Colors.Red, 0),
						new GradientStop(Colors.Blue, 1)
					},
					new Point(0, 0),
					new Point(1, 1)));
				var appearance = new ShellAppearance();
				appearance.Ingest(shellItem);
				tracker.SetAppearance(controller, appearance);
				Assert.Single(tabBar.Layer.Sublayers.OfType<CAGradientLayer>());

				tabBar.BarTintColor = Colors.Blue.ToPlatform();
				tabBar.UnselectedItemTintColor = Colors.Red.ToPlatform();
				tracker.SetAppearance(controller, appearance);
				tracker.ResetAppearance(controller);

				Assert.DoesNotContain(tabBar.Layer.Sublayers ?? Array.Empty<CALayer>(), layer => layer is CAGradientLayer);
				Assert.Null(tabBar.BackgroundColor);
				Assert.True(ColorComparison.ARGBEquivalent(Colors.Purple.ToPlatform(), tabBar.BarTintColor));
				Assert.True(ColorComparison.ARGBEquivalent(Colors.Green.ToPlatform(), tabBar.UnselectedItemTintColor));
			});
		}

		sealed class NullTintTabBar : UITabBar
		{
			public override UIColor TintColor
			{
				get => null;
				set { }
			}
		}

		sealed class TestTabBarController : UITabBarController
		{
			readonly UITabBar _tabBar;

			public TestTabBarController(UITabBar tabBar) => _tabBar = tabBar;

			public override UITabBar TabBar => _tabBar;
		}
	}
}
