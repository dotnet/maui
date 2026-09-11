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
				Shell.SetTabBarBackgroundColor(shell, Colors.White);
				Shell.SetTabBarForegroundColor(shell, Colors.Black);
				Shell.SetTabBarTitleColor(shell, Colors.Black);

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
					!NativeTabBarItemHasAnyBadgeTextColor(tab, badgeTextColor), timeout: 2000);
			});
		}

		[Fact(DisplayName = "Shell Tab Badge Colors Stay Independent Across Selected Tabs")]
		public async Task ShellTabBadgeColorsStayIndependentAcrossSelectedTabs()
		{
			SetupBuilder();

			var tab1BadgeColor = Colors.Blue;
			var tab2BadgeColor = Colors.Red;
			var tab1 = CreateBadgeTab("Tab1", "white_tab.png", tab1BadgeColor, Colors.White);
			var tab2 = CreateBadgeTab("Tab2", "white_tab.png", tab2BadgeColor, Colors.White);

			var shell = await CreateShellAsync(shell =>
			{
				shell.Items.Add(new TabBar()
				{
					Items =
					{
						tab1,
						tab2
					}
				});
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				await OnLoadedAsync(shell.CurrentPage);
				await AssertEventually(() =>
					NativeTabBarItemHasBadgeColors(tab1, tab1BadgeColor, Colors.White) &&
					NativeTabBarItemHasBadgeColors(tab2, tab2BadgeColor, Colors.White) &&
					!NativeTabBarItemHasBadgeAppearanceColors(tab1) &&
					!NativeTabBarItemHasBadgeAppearanceColors(tab2), timeout: 2000);

				await InvokeOnMainThreadAsync(() => shell.CurrentItem.CurrentItem = tab2);
				await AssertEventually(() =>
					NativeTabBarItemHasBadgeColors(tab1, tab1BadgeColor, Colors.White) &&
					NativeTabBarItemHasBadgeColors(tab2, tab2BadgeColor, Colors.White) &&
					!NativeTabBarItemHasBadgeAppearanceColors(tab1) &&
					!NativeTabBarItemHasBadgeAppearanceColors(tab2), timeout: 2000);
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

			if (!ColorComparison.ARGBEquivalent(tabBarItem.BadgeColor, badgeColor.ToPlatform()))
				return false;

			if (badgeTextColor is not null && !NativeTabBarItemHasBadgeTextColor(item, badgeTextColor))
			{
				return false;
			}

			return true;
		}

		bool NativeTabBarItemHasAnyBadgeTextColor(ShellSection item, Color badgeTextColor)
		{
			var tabBarItem = GetRendererTabBarItem(item);
			if (tabBarItem is null)
				return false;

			return BadgeTextAttributesHaveColor(tabBarItem, UIControlState.Normal, badgeTextColor) ||
				BadgeTextAttributesHaveColor(tabBarItem, UIControlState.Selected, badgeTextColor) ||
				BadgeTextAttributesHaveColor(tabBarItem, UIControlState.Disabled, badgeTextColor) ||
				BadgeTextAttributesHaveColor(tabBarItem, UIControlState.Focused, badgeTextColor);
		}

		bool NativeTabBarItemHasBadgeTextColor(ShellSection item, Color badgeTextColor)
		{
			var tabBarItem = GetRendererTabBarItem(item);
			if (tabBarItem is null)
				return false;

			return BadgeTextAttributesHaveColor(tabBarItem, UIControlState.Normal, badgeTextColor) &&
				BadgeTextAttributesHaveColor(tabBarItem, UIControlState.Selected, badgeTextColor) &&
				BadgeTextAttributesHaveColor(tabBarItem, UIControlState.Disabled, badgeTextColor) &&
				BadgeTextAttributesHaveColor(tabBarItem, UIControlState.Focused, badgeTextColor);
		}

		bool BadgeTextAttributesHaveColor(UITabBarItem tabBarItem, UIControlState state, Color badgeTextColor) =>
			ColorComparison.ARGBEquivalent(tabBarItem.GetBadgeTextAttributes(state)?.ForegroundColor, badgeTextColor.ToPlatform());

		bool NativeTabBarItemHasBadgeAppearanceColors(ShellSection item)
		{
			if (!OperatingSystem.IsIOSVersionAtLeast(15) && !OperatingSystem.IsMacCatalystVersionAtLeast(15))
				return false;

			var tabBarItem = GetRendererTabBarItem(item);
			return tabBarItem is not null &&
				(TabBarAppearanceHasBadgeColors(tabBarItem.StandardAppearance) ||
				TabBarAppearanceHasBadgeColors(tabBarItem.ScrollEdgeAppearance));
		}

		bool TabBarAppearanceHasBadgeColors(UITabBarAppearance appearance) =>
			appearance is not null &&
			(TabBarItemAppearanceHasBadgeColors(appearance.StackedLayoutAppearance) ||
			TabBarItemAppearanceHasBadgeColors(appearance.InlineLayoutAppearance) ||
			TabBarItemAppearanceHasBadgeColors(appearance.CompactInlineLayoutAppearance));

		bool TabBarItemAppearanceHasBadgeColors(UITabBarItemAppearance appearance) =>
			TabBarItemStateAppearanceHasBadgeColors(appearance.Normal) ||
			TabBarItemStateAppearanceHasBadgeColors(appearance.Selected) ||
			TabBarItemStateAppearanceHasBadgeColors(appearance.Disabled) ||
			TabBarItemStateAppearanceHasBadgeColors(appearance.Focused);

		bool TabBarItemStateAppearanceHasBadgeColors(UITabBarItemStateAppearance appearance) =>
			appearance.BadgeBackgroundColor is not null ||
			appearance.WeakBadgeTextAttributes?[UIStringAttributeKey.ForegroundColor] is not null;
	}
}
