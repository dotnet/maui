using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Xunit;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using WFrameworkElement = Microsoft.UI.Xaml.FrameworkElement;
using WNavigationViewItem = Microsoft.UI.Xaml.Controls.NavigationViewItem;


namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		[Fact(DisplayName = "ForegroundColor sets icon and title color sets title")]
		public async Task ForegroundColorSetsIconAndTitleColorSetsTitle()
		{
			SetupBuilder();

			var titleColor = Color.FromArgb("#FFFF0000");
			var foregroundColor = Color.FromArgb("#FF00FF00");
			await RunShellTabBarTests(shell =>
			{
				Shell.SetTabBarTitleColor(shell, titleColor);
				Shell.SetTabBarForegroundColor(shell, foregroundColor);
			},
			async (shell) =>
			{
				await ValidateTabBarItemColor(shell.CurrentSection, titleColor, true);
				await ValidateTabBarItemColor(shell.CurrentSection, foregroundColor, true);
				await ValidateTabBarItemColor(shell.Items[0].Items[1], titleColor, false);
				await ValidateTabBarItemColor(shell.Items[0].Items[1], foregroundColor, false);
			});
		}

		[Theory(DisplayName = "Shell TabBar Title Color Initializes Correctly")]
		[InlineData("#FFFF0000")]
		[InlineData("#FF00FF00")]

		public async Task ShellTabBarTitleColorInitializesCorrectly(string colorHex)
		{
			SetupBuilder();

			var expectedColor = Color.FromArgb(colorHex);
			await RunShellTabBarTests(shell => Shell.SetTabBarTitleColor(shell, expectedColor),
				async (shell) =>
				{
					await ValidateTabBarItemColor(shell.CurrentSection, expectedColor, true);
					await ValidateTabBarItemColor(shell.Items[0].Items[1], expectedColor, false);
				});
		}

		[Theory(DisplayName = "Shell TabBar Foreground Initializes Correctly")]
		[InlineData("#FFFF0000")]
		[InlineData("#FF00FF00")]
		public async Task ShellTabBarForegroundInitializesCorrectly(string colorHex)
		{
			var expectedColor = Color.FromArgb(colorHex);
			await RunShellTabBarTests(shell => Shell.SetTabBarForegroundColor(shell, expectedColor),
				async (shell) =>
				{
					await ValidateTabBarItemColor(shell.CurrentSection, expectedColor, true);
					await ValidateTabBarItemColor(shell.Items[0].Items[1], expectedColor, false);
				});
		}

		[Theory(DisplayName = "Shell TabBar UnselectedColor Initializes Correctly")]
		[InlineData("#FF0000")]
		[InlineData("#0000FF")]
		public async Task ShellTabBarUnselectedColorInitializesCorrectly(string colorHex)
		{
			var expectedColor = Color.FromArgb(colorHex);
			await RunShellTabBarTests(shell => Shell.SetTabBarUnselectedColor(shell, expectedColor),
				async (shell) =>
				{
					await ValidateTabBarItemColor(shell.CurrentSection, expectedColor, false);
					await ValidateTabBarItemColor(shell.Items[0].Items[1], expectedColor, true);
				});
		}

		async Task RunShellTabBarTests(Action<Shell> setup, Func<Shell, Task> runTest)
		{
			SetupBuilder();

			var selectedContent = new ShellContent()
			{
				Route = "Tab1",
				Title = "Tab1",
				Content = new ContentPage(),
				Icon = "white.png"
			};

			var unselectedContent = new ShellContent()
			{
				Route = "Tab2",
				Title = "Tab2",
				Content = new ContentPage(),
				Icon = "white.png"
			};

			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new TabBar()
				{
					Items =
					{
						selectedContent,
						unselectedContent,
					}
				});
			});

			setup.Invoke(shell);
			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await runTest(shell);
			});
		}

		List<WNavigationViewItem> GetTabBarItems(Shell shell)
		{
			var shellItemHandler = shell.CurrentItem.Handler as ShellItemHandler;
			var navView = shellItemHandler.PlatformView as MauiNavigationView;

			return navView.TopNavArea.GetChildren<WNavigationViewItem>().ToList();
		}
		async Task ValidateTabBarItemColor(ShellSection item, Color expectedColor, bool hasColor)
		{
			var items = GetTabBarItems(item.FindParentOfType<Shell>());
			var platformItem =
				items.FirstOrDefault(x => x.Content.ToString().Equals(item.Title, StringComparison.OrdinalIgnoreCase));

			if (hasColor)
				await AssertionExtensions.AssertContainsColor(platformItem, expectedColor, item.FindMauiContext());
			else
				await AssertionExtensions.AssertDoesNotContainColor(platformItem, expectedColor, item.FindMauiContext());
		}
	}
}
