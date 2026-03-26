using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

#if ANDROID || IOS || MACCATALYST
using ShellHandler = Microsoft.Maui.Controls.Handlers.Compatibility.ShellRenderer;
#elif WINDOWS
using Microsoft.Maui.Controls.Handlers;
#endif

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		[Fact(DisplayName = "ForegroundColor sets icon and title color sets title")]
		public async Task ForegroundColorSetsIconAndTitleColorSetsTitle()
		{
#if IOS || MACCATALYST
			// Pixel-based tab bar color verification doesn't work on iOS 26+ due to UITabBar rendering changes
			if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
				return;
#endif
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
				await ValidateTabBarIconColor(shell.CurrentSection, foregroundColor, true);
				await ValidateTabBarTextColor(shell.CurrentSection, titleColor, true);

				await ValidateTabBarIconColor(shell.Items[0].Items[1], titleColor, false);
				await ValidateTabBarTextColor(shell.Items[0].Items[1], foregroundColor, false);
			});
		}

		[Theory(DisplayName = "Shell TabBar Title Color Initializes Correctly")]
		[InlineData("#FFFF0000")]
		[InlineData("#FF00FF00")]

		public async Task ShellTabBarTitleColorInitializesCorrectly(string colorHex)
		{
#if IOS || MACCATALYST
			// Pixel-based tab bar color verification doesn't work on iOS 26+ due to UITabBar rendering changes
			if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
				return;
#endif
			SetupBuilder();

			var expectedColor = Color.FromArgb(colorHex);
			await RunShellTabBarTests(shell => Shell.SetTabBarTitleColor(shell, expectedColor),
				async (shell) =>
				{
					await ValidateTabBarTextColor(shell.CurrentSection, expectedColor, true);
					await ValidateTabBarIconColor(shell.CurrentSection, expectedColor, true);
					await ValidateTabBarTextColor(shell.Items[0].Items[1], expectedColor, false);
					await ValidateTabBarIconColor(shell.Items[0].Items[1], expectedColor, false);
				});
		}

		[Theory(DisplayName = "Shell TabBar Foreground Initializes Correctly")]
		[InlineData("#FFFF0000")]
		[InlineData("#FF00FF00")]
		public async Task ShellTabBarForegroundInitializesCorrectly(string colorHex)
		{
#if IOS || MACCATALYST
			// Pixel-based tab bar color verification doesn't work on iOS 26+ due to UITabBar rendering changes
			if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
				return;
#endif
			var expectedColor = Color.FromArgb(colorHex);
			await RunShellTabBarTests(shell => Shell.SetTabBarForegroundColor(shell, expectedColor),
				async (shell) =>
				{
					await ValidateTabBarTextColor(shell.CurrentSection, expectedColor, true);
					await ValidateTabBarIconColor(shell.CurrentSection, expectedColor, true);
					await ValidateTabBarTextColor(shell.Items[0].Items[1], expectedColor, false);
					await ValidateTabBarIconColor(shell.Items[0].Items[1], expectedColor, false);
				});
		}

		[Theory(DisplayName = "Shell TabBar UnselectedColor Initializes Correctly")]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		public async Task ShellTabBarUnselectedColorInitializesCorrectly(string colorHex)
		{
#if IOS || MACCATALYST
			// Pixel-based tab bar color verification doesn't work on iOS 26+ due to UITabBar rendering changes
			if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
				return;
#endif
			var expectedColor = Color.FromArgb(colorHex);
			await RunShellTabBarTests(shell => Shell.SetTabBarUnselectedColor(shell, expectedColor),
				async (shell) =>
				{
					await ValidateTabBarTextColor(shell.CurrentSection, expectedColor, false);
					await ValidateTabBarIconColor(shell.CurrentSection, expectedColor, false);
					await ValidateTabBarTextColor(shell.Items[0].Items[1], expectedColor, true);
					await ValidateTabBarIconColor(shell.Items[0].Items[1], expectedColor, true);
				});
		}

		// Regression test for https://github.com/dotnet/maui/issues/32125
		// On iOS 26+, Shell.TabBarUnselectedColor was not applied to unselected tabs
		[Fact(DisplayName = "Shell TabBar UnselectedColor and TitleColor work together")]
		public async Task ShellTabBarUnselectedAndTitleColorWorkTogether()
		{
			var titleColor = Color.FromArgb("#FFFF0000");
			var unselectedColor = Color.FromArgb("#FF808080");
			await RunShellTabBarTests(shell =>
			{
				Shell.SetTabBarTitleColor(shell, titleColor);
				Shell.SetTabBarUnselectedColor(shell, unselectedColor);
			},
			async (shell) =>
			{
#if IOS || MACCATALYST
				if (OperatingSystem.IsIOSVersionAtLeast(26) || OperatingSystem.IsMacCatalystVersionAtLeast(26))
				{
					// On iOS 26+, verify the UnselectedItemTintColor property directly
					// because pixel-based verification doesn't work with the new tab bar rendering
					await ValidateTabBarUnselectedTintColorProperty(shell.CurrentSection, unselectedColor);
				}
				else
#endif
				{
					// Selected tab should use title color
					await ValidateTabBarTextColor(shell.CurrentSection, titleColor, true);
					// Unselected tab should use unselected color
					await ValidateTabBarTextColor(shell.Items[0].Items[1], unselectedColor, true);
					await ValidateTabBarIconColor(shell.Items[0].Items[1], unselectedColor, true);
				}
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
				Icon = "white_tab.png"
			};

			var unselectedContent = new ShellContent()
			{
				Route = "Tab2",
				Title = "Tab2",
				Content = new ContentPage(),
				Icon = "white_tab.png"
			};

			var unselectedContent_2 = new ShellContent()
			{
				Route = "Tab3",
				Title = "Tab3",
				Content = new ContentPage(),
				Icon = "white_tab.png"
			};

			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new TabBar()
				{
					Items =
					{
						selectedContent,
						unselectedContent,
						unselectedContent_2
					}
				});
			});

			setup.Invoke(shell);
			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				await runTest(shell);
			});
		}
	}
}
