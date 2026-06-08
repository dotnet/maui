using System;
using System.Linq;
using System.Threading.Tasks;
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
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class ShellBadgeTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "Shell Tab Badge Colors Apply To Native Tab Bar Item")]
		public async Task ShellTabBadgeColorsApplyToNativeTabBarItem()
		{
			SetupBuilder();

			var badgeColor = Color.FromArgb("#00C853");
			var badgeTextColor = Color.FromArgb("#111111");
			var tab = CreateBadgeTab("Tab1", "white_tab.png", badgeColor, badgeTextColor);

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new TabBar()
				{
					Items =
					{
						tab,
						CreateBadgeTab("Tab2", "white_tab.png", Colors.Red, Colors.White)
					}
				});
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await AssertEventually(() => NativeTabBarItemHasBadgeColors(tab, badgeColor, badgeTextColor), timeout: 2000);
			});
		}

		[Fact(DisplayName = "Shell Tab Badge Colors Persist After Native Tab Bar Item Updates")]
		public async Task ShellTabBadgeColorsPersistAfterNativeTabBarItemUpdates()
		{
			SetupBuilder();

			var badgeColor = Color.FromArgb("#00C853");
			var badgeTextColor = Color.FromArgb("#111111");
			var tab = CreateBadgeTab("Tab1", "white_tab.png", badgeColor, badgeTextColor);

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new TabBar()
				{
					Items =
					{
						tab,
						CreateBadgeTab("Tab2", "white_tab.png", Colors.Red, Colors.White)
					}
				});
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await AssertEventually(() => NativeTabBarItemHasBadgeColors(tab, badgeColor, badgeTextColor), timeout: 2000);

				tab.Title = "UpdatedTab1";
				tab.Icon = "green.png";

				await AssertEventually(() =>
				{
					var tabBarItem = GetRendererTabBarItem(tab);
					return tabBarItem?.Title == "UpdatedTab1" &&
						NativeTabBarItemHasBadgeColors(tab, badgeColor, badgeTextColor);
				}, timeout: 2000);
			});
		}

		[Fact(DisplayName = "Shell Tab Badge Background Color Applies With Default Text Color")]
		public async Task ShellTabBadgeBackgroundColorAppliesWithDefaultTextColor()
		{
			SetupBuilder();

			var badgeColor = Color.FromArgb("#00C853");
			var tab = CreateBadgeTab("Tab1", "white_tab.png", badgeColor, null);

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new TabBar()
				{
					Items =
					{
						tab,
						CreateBadgeTab("Tab2", "white_tab.png", Colors.Red, Colors.White)
					}
				});
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await AssertEventually(() => NativeTabBarItemHasBadgeColors(tab, badgeColor, null), timeout: 2000);
			});
		}

		[Fact(DisplayName = "Shell Tab Badge Text Color Clears To Default")]
		public async Task ShellTabBadgeTextColorClearsToDefault()
		{
			SetupBuilder();

			var badgeColor = Color.FromArgb("#00C853");
			var badgeTextColor = Color.FromArgb("#FF00FF");
			var tab = CreateBadgeTab("Tab1", "white_tab.png", badgeColor, badgeTextColor);

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new TabBar()
				{
					Items =
					{
						tab,
						CreateBadgeTab("Tab2", "white_tab.png", Colors.Red, Colors.White)
					}
				});
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await AssertEventually(() => NativeTabBarItemHasBadgeColors(tab, badgeColor, badgeTextColor), timeout: 2000);

				tab.BadgeTextColor = null;

				await AssertEventually(() =>
					NativeTabBarItemHasBadgeColors(tab, badgeColor, null) &&
					!NativeTabBarItemHasBadgeAppearanceTextColor(tab, badgeTextColor), timeout: 2000);
			});
		}

		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.SetupShellHandlers();
			});
		}

		Task<Shell> CreateShellAsync(Action<Shell> action) =>
			InvokeOnMainThreadAsync(() =>
			{
				var shell = new Shell();
				action(shell);
				return shell;
			});

		Tab CreateBadgeTab(string title, string icon, Color badgeColor, Color badgeTextColor)
		{
			return new Tab()
			{
				Title = title,
				Icon = icon,
				BadgeText = "7",
				BadgeColor = badgeColor,
				BadgeTextColor = badgeTextColor,
				Items =
				{
					new ShellContent()
					{
						Title = title,
						Content = new ContentPage()
					}
				}
			};
		}

		UITabBarItem GetRendererTabBarItem(ShellSection item)
		{
			var shellItem = item.Parent as ShellItem;
			var shell = shellItem.Parent as Shell;
			var shellContext = shell.Handler as IShellContext;
			var shellItemRenderer = shellContext?.CurrentShellItemRenderer as ShellItemRenderer;
			var sectionRenderer = shellItemRenderer?.ViewControllers
				.OfType<ShellSectionRenderer>()
				.FirstOrDefault(renderer => renderer.ShellSection == item);

			return sectionRenderer?.ViewController.TabBarItem;
		}

		bool NativeTabBarItemHasBadgeColors(ShellSection item, Color badgeColor, Color badgeTextColor)
		{
			var tabBarItem = GetRendererTabBarItem(item);
			if (tabBarItem is null || tabBarItem.BadgeValue != item.BadgeText)
				return false;

			var badgeAttributes = tabBarItem.GetBadgeTextAttributes(UIControlState.Normal);

			if (!ColorComparison.ARGBEquivalent(tabBarItem.BadgeColor, badgeColor.ToPlatform()))
				return false;

			if (badgeTextColor is not null && !ColorComparison.ARGBEquivalent(badgeAttributes?.ForegroundColor, badgeTextColor.ToPlatform()))
			{
				return false;
			}

			return NativeTabBarItemHasBadgeAppearance(tabBarItem, badgeColor, badgeTextColor);
		}

		bool NativeTabBarItemHasBadgeAppearance(UITabBarItem tabBarItem, Color badgeColor, Color badgeTextColor)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15))
				return true;

			return TabBarAppearanceHasBadgeColors(tabBarItem.StandardAppearance, badgeColor, badgeTextColor) &&
				TabBarAppearanceHasBadgeColors(tabBarItem.ScrollEdgeAppearance, badgeColor, badgeTextColor);
		}

		bool NativeTabBarItemHasBadgeAppearanceTextColor(ShellSection item, Color badgeTextColor)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15))
				return false;

			var tabBarItem = GetRendererTabBarItem(item);
			return tabBarItem is not null &&
				(TabBarAppearanceHasBadgeTextColor(tabBarItem.StandardAppearance, badgeTextColor) ||
				TabBarAppearanceHasBadgeTextColor(tabBarItem.ScrollEdgeAppearance, badgeTextColor));
		}

		bool TabBarAppearanceHasBadgeColors(UITabBarAppearance appearance, Color badgeColor, Color badgeTextColor)
		{
			if (appearance is null)
				return false;

			return TabBarItemAppearanceHasBadgeColors(appearance.StackedLayoutAppearance, badgeColor, badgeTextColor) &&
				TabBarItemAppearanceHasBadgeColors(appearance.InlineLayoutAppearance, badgeColor, badgeTextColor) &&
				TabBarItemAppearanceHasBadgeColors(appearance.CompactInlineLayoutAppearance, badgeColor, badgeTextColor);
		}

		bool TabBarItemAppearanceHasBadgeColors(UITabBarItemAppearance appearance, Color badgeColor, Color badgeTextColor)
		{
			return TabBarItemStateAppearanceHasBadgeColors(appearance.Normal, badgeColor, badgeTextColor) &&
				TabBarItemStateAppearanceHasBadgeColors(appearance.Selected, badgeColor, badgeTextColor) &&
				TabBarItemStateAppearanceHasBadgeColors(appearance.Disabled, badgeColor, badgeTextColor) &&
				TabBarItemStateAppearanceHasBadgeColors(appearance.Focused, badgeColor, badgeTextColor);
		}

		bool TabBarAppearanceHasBadgeTextColor(UITabBarAppearance appearance, Color badgeTextColor)
		{
			return appearance is not null &&
				(TabBarItemAppearanceHasBadgeTextColor(appearance.StackedLayoutAppearance, badgeTextColor) ||
				TabBarItemAppearanceHasBadgeTextColor(appearance.InlineLayoutAppearance, badgeTextColor) ||
				TabBarItemAppearanceHasBadgeTextColor(appearance.CompactInlineLayoutAppearance, badgeTextColor));
		}

		bool TabBarItemAppearanceHasBadgeTextColor(UITabBarItemAppearance appearance, Color badgeTextColor)
		{
			return TabBarItemStateAppearanceHasBadgeTextColor(appearance.Normal, badgeTextColor) ||
				TabBarItemStateAppearanceHasBadgeTextColor(appearance.Selected, badgeTextColor) ||
				TabBarItemStateAppearanceHasBadgeTextColor(appearance.Disabled, badgeTextColor) ||
				TabBarItemStateAppearanceHasBadgeTextColor(appearance.Focused, badgeTextColor);
		}

		bool TabBarItemStateAppearanceHasBadgeColors(UITabBarItemStateAppearance appearance, Color badgeColor, Color badgeTextColor)
		{
			if (!ColorComparison.ARGBEquivalent(appearance.BadgeBackgroundColor, badgeColor.ToPlatform()))
				return false;

			var foregroundColor = appearance.WeakBadgeTextAttributes?[UIStringAttributeKey.ForegroundColor] as UIColor;

			if (badgeTextColor is null)
				return true;

			return ColorComparison.ARGBEquivalent(foregroundColor, badgeTextColor.ToPlatform());
		}

		bool TabBarItemStateAppearanceHasBadgeTextColor(UITabBarItemStateAppearance appearance, Color badgeTextColor)
		{
			var foregroundColor = appearance.WeakBadgeTextAttributes?[UIStringAttributeKey.ForegroundColor] as UIColor;
			return ColorComparison.ARGBEquivalent(foregroundColor, badgeTextColor.ToPlatform());
		}
	}
}
