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
