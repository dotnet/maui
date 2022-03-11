using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;


namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests : HandlerTestBase
	{
		[Fact(DisplayName = "Shell with Single Content Page Has Panes Disabled")]
		public async Task BasicShellHasPaneDisplayModeDisabled()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) => {
				shell.Items.Add(new ContentPage());
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				var rootNavView = (handler.PlatformView);
				var shellItemView = shell.CurrentItem.Handler.PlatformView as UI.Xaml.Controls.NavigationView;
				var expected = UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;

				Assert.False(rootNavView.IsPaneToggleButtonVisible);
				Assert.False(shellItemView.IsPaneToggleButtonVisible);

				Assert.Equal(expected, rootNavView.PaneDisplayMode);
				Assert.Equal(expected, shellItemView.PaneDisplayMode);

				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Shell With Only Flyout Items")]
		public async Task ShellWithOnlyFlyoutItems()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) => {
				var shellItem1 = new FlyoutItem();
				shellItem1.Items.Add(new ContentPage());

				var shellItem2 = new FlyoutItem();
				shellItem2.Items.Add(new ContentPage());

				shell.Items.Add(shellItem1);
				shell.Items.Add(shellItem2);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				var rootNavView = (handler.PlatformView);
				var shellItemView = shell.CurrentItem.Handler.PlatformView as UI.Xaml.Controls.NavigationView;
				var expectedRoot = UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;
				var expectedShellItems = UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;

				Assert.True(rootNavView.IsPaneToggleButtonVisible);
				Assert.False(shellItemView.IsPaneToggleButtonVisible);
				Assert.Equal(expectedRoot, rootNavView.PaneDisplayMode);
				Assert.Equal(expectedShellItems, shellItemView.PaneDisplayMode);

				return Task.CompletedTask;
			});
		}


		[Fact(DisplayName = "Shell With Only Top Tabs")]
		public async Task ShellWithOnlyTopTabs()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) => {
				var shellItem = new TabBar();
				var shellSection1 = new ShellSection();
				shellSection1.Items.Add(new ContentPage());

				var shellSection2 = new ShellSection();
				shellSection2.Items.Add(new ContentPage());

				shellItem.Items.Add(shellSection1);
				shellItem.Items.Add(shellSection2);

				shell.Items.Add(shellItem);
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				var rootNavView = (handler.PlatformView);
				var shellItemView = shell.CurrentItem.Handler.PlatformView as UI.Xaml.Controls.NavigationView;
				var expectedRoot = UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftMinimal;
				var expectedShellItems = UI.Xaml.Controls.NavigationViewPaneDisplayMode.Top;

				Assert.False(rootNavView.IsPaneToggleButtonVisible);
				Assert.False(shellItemView.IsPaneToggleButtonVisible);
				Assert.Equal(expectedRoot, rootNavView.PaneDisplayMode);
				Assert.Equal(expectedShellItems, shellItemView.PaneDisplayMode);

				return Task.CompletedTask;
			});
		}


		[Fact(DisplayName = "Flyout Locked Offset")]
		public async Task FlyoutLockedOffset()
		{
			SetupBuilder();
			var label = new StackLayout()
			{ 
				HeightRequest = 10
			};

			var shell = await InvokeOnMainThreadAsync(() => { 
				return new Shell()
				{
					FlyoutHeader = label,
					Items =
					{
						new ContentPage()
					},
					FlyoutBehavior = FlyoutBehavior.Locked
				};
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				var rootManager = handler.MauiContext.GetNavigationRootManager();
				var position = label.GetLocationRelativeTo(rootManager.AppTitleBar);
				var distance = rootManager.AppTitleBar.Height - position.Value.Y;
				Assert.True(Math.Abs(distance) < 1);
				return Task.CompletedTask;
			});
		}
	}
}
