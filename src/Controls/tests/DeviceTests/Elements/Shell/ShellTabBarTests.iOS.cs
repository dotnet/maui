using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		[Fact]
		public async Task TabBarVisibleAfterNavigationFromHiddenRoot26598()
		{
			SetupBuilder();

			var homeButton = new Button { Text = "Navigate to InnerTab" };
			var innerButton = new Button { Text = "Navigate to TabBarPage" };
			var recentLabel = new Label { Text = "Page Loaded in Recent Tab" };
			var home = CreatePage("Home", homeButton, false, 200);
			var inner = CreatePage("InnerTab", innerButton, true);
			var recent = CreatePage("Recent", recentLabel, true, 200);

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new TabBar
				{
					Items =
					{
						new ShellContent { Title = "HomeTab", ContentTemplate = new DataTemplate(() => home) },
						new ShellContent { Title = "RecentTab", ContentTemplate = new DataTemplate(() => recent) }
					}
				});
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async renderer =>
			{
				var itemRenderer = ((IShellContext)renderer).CurrentShellItemRenderer;
				var controller = Assert.IsAssignableFrom<UITabBarController>(itemRenderer?.ViewController);
				var tabBar = controller.TabBar;
				Assert.NotNull(tabBar);
				var hasTabBarHidden = OperatingSystem.IsIOSVersionAtLeast(18)
					|| OperatingSystem.IsMacCatalystVersionAtLeast(18);

				await AssertState("Initial hidden Home", home, homeButton, 0, false, 1);

				await shell.Navigation.PushAsync(inner);
				await AssertState("First navigation to InnerTab", inner, innerButton, 0, true, 2);

				async Task AssertState(string stage, ContentPage page, View content, int index, bool visible, int stackDepth)
				{
					var expectedText = content switch
					{
						Button button => button.Text,
						Label label => label.Text,
						_ => throw new InvalidOperationException("Issue26598 requires a button or label as page content.")
					};

					var matched = await Wait(() =>
					{
						var nativeContent = content.Handler?.PlatformView as UIView;
						var nativeText = nativeContent switch
						{
							UIButton button => button.CurrentTitle,
							UILabel label => label.Text,
							_ => null
						};
						var navigation = controller.SelectedViewController as UINavigationController;
						var top = navigation?.TopViewController;

						return shell.CurrentPage == page
							&& shell.CurrentSection == shell.Items[0].Items[index]
							&& controller.ViewControllers?.Length == 2
							&& controller.SelectedViewController == controller.ViewControllers[index]
							&& navigation?.ViewControllers?.Length == stackDepth
							&& top?.ViewIfLoaded is UIView topView
							&& navigation.VisibleViewController == top
							&& HasVisibleGeometry(nativeContent)
							&& nativeContent.IsDescendantOfView(topView)
							&& nativeText == expectedText
							&& tabBar.Items?.Length == 2
							&& tabBar.Items[0].Title == "HomeTab"
							&& tabBar.Items[1].Title == "RecentTab"
							&& (!visible || tabBar.SelectedItem == tabBar.Items[index])
							&& (!hasTabBarHidden || controller.TabBarHidden == !visible)
							&& IsTabBarHittable() == visible;
					}, timeout: 2000);

					Assert.True(matched,
						$"Issue26598 {stage}: expected page={page.Title}, tab index={index}, visible={visible}, " +
						$"native stack depth={stackDepth}, content='{expectedText}'. {DescribeState()} " +
						$"Expected content ancestry: {DescribeAncestry(content.Handler?.PlatformView as UIView)}");
				}

				UIView GetTabBarHitTarget()
				{
					if (!HasVisibleGeometry(tabBar))
						return null;

					var center = new CGPoint(tabBar.Bounds.GetMidX(), tabBar.Bounds.GetMidY());
					return tabBar.Window.HitTest(tabBar.ConvertPointToView(center, tabBar.Window), null);
				}

				bool IsTabBarHittable()
				{
					var hit = GetTabBarHitTarget();
					return hit == tabBar || hit?.IsDescendantOfView(tabBar) == true;
				}

				string DescribeState()
				{
					var navigation = controller.SelectedViewController as UINavigationController;
					var hidden = hasTabBarHidden ? controller.TabBarHidden.ToString() : "unavailable";
					return $"page={shell.CurrentPage?.Title}, section={shell.CurrentSection?.Title}, " +
						$"controller={controller.GetType().Name}, TabBarHidden={hidden}, " +
						$"selectedIndex={controller.SelectedIndex}, selectedItem={tabBar.SelectedItem?.Title}, " +
						$"items=[{string.Join(", ", tabBar.Items?.Select(item => item.Title) ?? Array.Empty<string>())}], " +
						$"selectedController={controller.SelectedViewController?.GetType().Name}, " +
						$"nativeStackDepth={navigation?.ViewControllers?.Length}, top={navigation?.TopViewController?.Title}, " +
						$"visibleController={navigation?.VisibleViewController?.Title}, " +
						$"HidesBottomBarWhenPushed={navigation?.TopViewController?.HidesBottomBarWhenPushed}, " +
						$"tabBarGeometryVisible={HasVisibleGeometry(tabBar)}, " +
						$"tabBarHitTarget={GetTabBarHitTarget()?.GetType().Name ?? "null"}, " +
						$"tab bar ancestry: {DescribeAncestry(tabBar)}";
				}
			});

			static ContentPage CreatePage(string title, View content, bool visible, double height = -1)
			{
				content.HorizontalOptions = LayoutOptions.Center;
				content.VerticalOptions = LayoutOptions.Center;
				var page = new ContentPage
				{
					Title = title,
					HeightRequest = height,
					Content = new VerticalStackLayout { content }
				};
				Shell.SetTabBarIsVisible(page, visible);
				return page;
			}

			static bool HasVisibleGeometry(UIView view)
			{
				if (view?.Window is not UIWindow window || view.Bounds.Width <= 0 || view.Bounds.Height <= 0)
					return false;

				var visibleBounds = CGRect.Intersect(view.ConvertRectToView(view.Bounds, window), window.Bounds);
				for (var current = view; current is not null; current = current.Superview)
				{
					if (current.Hidden || current.Alpha <= 0 || current.Layer.Hidden || current.Layer.Opacity <= 0)
						return false;

					if (current.ClipsToBounds)
						visibleBounds = CGRect.Intersect(visibleBounds, current.ConvertRectToView(current.Bounds, window));

					if (visibleBounds.Width <= 0 || visibleBounds.Height <= 0)
						return false;

					if (current == window)
						return true;
				}

				return false;
			}

			static string DescribeAncestry(UIView view)
			{
				var description = new StringBuilder();
				for (var current = view; current is not null; current = current.Superview)
				{
					description.Append($"{current.GetType().Name}[Hidden={current.Hidden}, Alpha={current.Alpha}, " +
						$"Frame={current.Frame}, Bounds={current.Bounds}, ClipsToBounds={current.ClipsToBounds}, " +
						$"LayerHidden={current.Layer.Hidden}, Opacity={current.Layer.Opacity}, " +
						$"Superview={current.Superview?.GetType().Name ?? "null"}, " +
						$"Window={current.Window?.Handle.ToString() ?? "null"}, WindowBounds={current.Window?.Bounds}] -> ");
				}

				return description.Length == 0 ? "unattached/null" : description.ToString();
			}
		}

		UITabBar GetTabBar(ShellSection item)
		{
			var shellItem = item.Parent as ShellItem;
			var shell = shellItem.Parent as Shell;

			var pagerParent = (shell.CurrentPage.Handler as IPlatformViewHandler)
				.PlatformView.FindParent(x => x.NextResponder is UITabBarController);

			// In macOS 15 Sequoia, the UITabBar is nested within the second subview (index 1) of the pagerParent. 
			if (OperatingSystem.IsMacCatalystVersionAtLeast(15, 0) || OperatingSystem.IsMacOSVersionAtLeast(15, 0))
			{
				var subview = pagerParent.Subviews.ElementAtOrDefault(1);

				if (subview?.Subviews is null)
					return null;

				return subview.Subviews.OfType<UITabBar>().FirstOrDefault();
			}

			return pagerParent.Subviews.OfType<UITabBar>().FirstOrDefault();
		}

		async Task ValidateTabBarIconColor(
			ShellSection item,
			Color iconColor,
			bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemIconContainsColor(GetTabBar(item),
					item.Title, iconColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemIconDoesNotContainColor(GetTabBar(item),
					item.Title, iconColor, MauiContext);
			}
		}

		async Task ValidateTabBarTextColor(
				ShellSection item,
				Color textColor,
				bool hasColor)
		{
			if (hasColor)
			{
				await AssertionExtensions.AssertTabItemTextContainsColor(GetTabBar(item),
					item.Title, textColor, MauiContext);
			}
			else
			{
				await AssertionExtensions.AssertTabItemTextDoesNotContainColor(GetTabBar(item),
					item.Title, textColor, MauiContext);
			}
		}
	}
}