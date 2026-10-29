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
		[Fact]
		public async Task NavigationGradientRefreshesWithTabBarColorOverride()
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(13) && !OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
				return;

			await InvokeOnMainThreadAsync(() =>
			{
				using var controller = new UINavigationController();
				controller.NavigationBar.Frame = new CoreGraphics.CGRect(0, 0, 320, 44);
				using var tracker = new ShellNavBarAppearanceTracker();
				var observer = new NavigationAppearanceObserver(controller, tracker);
				var shell = new Shell();
				var section = new ShellSection
				{
					Items = { new ShellContent { Content = new ContentPage() } }
				};
				var shellItem = new ShellItem { Items = { section }, CurrentItem = section };
				shell.Items.Add(shellItem);
				var gradient = new LinearGradientBrush(
					new GradientStopCollection
					{
						new GradientStop(Colors.Red, 0),
						new GradientStop(Colors.Blue, 1)
					},
					new Point(0, 0),
					new Point(1, 1));
				Shell.SetBackground(shell, gradient);
				Shell.SetTabBarBackgroundColor(shell, Colors.Green);
				var shellController = (IShellController)shell;
				shellController.AddAppearanceObserver(observer, shellItem);
				using var initialImage = controller.NavigationBar.StandardAppearance.BackgroundImage.AsPNG();

				gradient.GradientStops[0].Color = Colors.Yellow;

				using var updatedImage = controller.NavigationBar.StandardAppearance.BackgroundImage.AsPNG();
				Assert.NotEqual(initialImage.ToArray(), updatedImage.ToArray());
				Assert.Equal(Colors.Green, observer.Appearance.TabBarBackgroundColor);
				Assert.True(shellController.RemoveAppearanceObserver(observer));
			});
		}

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
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public async Task ResetRestoresNativeAppearanceAfterGradientAndSolid(bool nullTint, bool hasScrollEdgeAppearance)
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
				navigationBar.ScrollEdgeAppearance = hasScrollEdgeAppearance ? scrollEdge : null;
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
				var translucentBeforeReset = navigationBar.Translucent;

				tracker.ResetAppearance(controller);

				Assert.True(ColorComparison.ARGBEquivalent(Colors.Purple.ToPlatform(), navigationBar.StandardAppearance.BackgroundColor));
				Assert.NotNull(navigationBar.ScrollEdgeAppearance);
				Assert.True(ColorComparison.ARGBEquivalent((hasScrollEdgeAppearance ? Colors.Green : Colors.Purple).ToPlatform(), navigationBar.ScrollEdgeAppearance.BackgroundColor));
				Assert.NotNull(navigationBar.StandardAppearance.BackgroundEffect);
				if (hasScrollEdgeAppearance)
					Assert.Null(navigationBar.ScrollEdgeAppearance.BackgroundEffect);
				else
					Assert.NotNull(navigationBar.ScrollEdgeAppearance.BackgroundEffect);
				Assert.Null(navigationBar.StandardAppearance.BackgroundImage);
				Assert.Null(navigationBar.ScrollEdgeAppearance.BackgroundImage);
				Assert.Equal(translucentBeforeReset, navigationBar.Translucent);
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

		sealed class NavigationAppearanceObserver : IAppearanceObserver
		{
			readonly UINavigationController _controller;
			readonly ShellNavBarAppearanceTracker _tracker;

			public NavigationAppearanceObserver(UINavigationController controller, ShellNavBarAppearanceTracker tracker)
			{
				_controller = controller;
				_tracker = tracker;
			}

			public ShellAppearance Appearance { get; private set; }

			public void OnAppearanceChanged(ShellAppearance appearance)
			{
				Appearance = appearance;
				_tracker.SetAppearance(_controller, appearance);
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
