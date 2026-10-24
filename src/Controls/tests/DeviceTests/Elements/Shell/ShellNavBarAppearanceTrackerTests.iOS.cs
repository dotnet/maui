using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public class ShellNavBarAppearanceTrackerTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData("Solid")]
		[InlineData("Linear")]
		[InlineData("Radial")]
		public async Task BrushUpdatesStandardAndScrollEdgeAppearance(string kind)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(13) && !OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
				return;

			await InvokeOnMainThreadAsync(() =>
			{
				using var controller = new UINavigationController();
				var navigationBar = controller.NavigationBar;
				navigationBar.Frame = new CoreGraphics.CGRect(0, 0, 320, 44);
				using var tracker = new ShellNavBarAppearanceTracker();
				Brush brush = kind switch
				{
					"Solid" => new SolidColorBrush(Colors.Blue),
					"Linear" => new LinearGradientBrush(),
					"Radial" => new RadialGradientBrush(),
					_ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
				};
				if (brush is GradientBrush gradient)
				{
					gradient.GradientStops.Add(new GradientStop(Colors.Red, 0));
					gradient.GradientStops.Add(new GradientStop(Colors.Blue, 1));
				}
				var shellItem = new ShellItem();
				Shell.SetBackground(shellItem, brush);
				var appearance = new ShellAppearance();
				appearance.Ingest(shellItem);

				tracker.SetAppearance(controller, appearance);

				if (brush is SolidColorBrush)
				{
					Assert.True(ColorComparison.ARGBEquivalent(Colors.Blue.ToPlatform(), navigationBar.StandardAppearance.BackgroundColor));
					Assert.True(ColorComparison.ARGBEquivalent(Colors.Blue.ToPlatform(), navigationBar.ScrollEdgeAppearance.BackgroundColor));
				}
				else
				{
					Assert.NotNull(navigationBar.StandardAppearance.BackgroundImage);
					Assert.NotNull(navigationBar.ScrollEdgeAppearance.BackgroundImage);
				}
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ResetRestoresNativeAppearanceAfterGradientAndSolid(bool nullTint)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(13) && !OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
				return;

			await InvokeOnMainThreadAsync(() =>
			{
				using var navigationBar = nullTint ? new NullTintNavigationBar() : new UINavigationBar();
				using var controller = new TestNavigationController(navigationBar);
				using var standard = new UINavigationBarAppearance();
				using var scrollEdge = new UINavigationBarAppearance();
				using var backgroundEffect = UIBlurEffect.FromStyle(UIBlurEffectStyle.Light);
				standard.ConfigureWithDefaultBackground();
				standard.BackgroundEffect = backgroundEffect;
				standard.BackgroundColor = Colors.Purple.ToPlatform();
				scrollEdge.ConfigureWithTransparentBackground();
				scrollEdge.BackgroundColor = Colors.Green.ToPlatform();
				navigationBar.StandardAppearance = standard;
				navigationBar.ScrollEdgeAppearance = scrollEdge;
				navigationBar.Translucent = true;
				using var tracker = new ShellNavBarAppearanceTracker();
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

				Assert.True(ColorComparison.ARGBEquivalent(Colors.Purple.ToPlatform(), navigationBar.StandardAppearance.BackgroundColor));
				Assert.True(ColorComparison.ARGBEquivalent(Colors.Green.ToPlatform(), navigationBar.ScrollEdgeAppearance.BackgroundColor));
				Assert.NotNull(navigationBar.StandardAppearance.BackgroundEffect);
				Assert.Null(navigationBar.ScrollEdgeAppearance.BackgroundEffect);
				Assert.Null(navigationBar.StandardAppearance.BackgroundImage);
				Assert.True(navigationBar.Translucent);
			});
		}

		sealed class NullTintNavigationBar : UINavigationBar
		{
			public override UIColor TintColor
			{
				get => null;
				set { }
			}
		}

		sealed class TestNavigationController : UINavigationController
		{
			readonly UINavigationBar _navigationBar;

			public TestNavigationController(UINavigationBar navigationBar) => _navigationBar = navigationBar;

			public override UINavigationBar NavigationBar => _navigationBar ?? base.NavigationBar;
		}
	}
}
