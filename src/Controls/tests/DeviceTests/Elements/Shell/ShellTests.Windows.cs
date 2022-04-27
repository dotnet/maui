using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Platform;
using Xunit;


namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests : HandlerTestBase
	{
		protected Task CheckFlyoutState(ShellHandler handler, bool desiredState)
		{
			Assert.Equal(desiredState, handler.PlatformView.IsPaneOpen);
			return Task.CompletedTask;
		}

		[Fact(DisplayName = "Back Button Enabled/Disabled")]
		public async Task BackButtonEnabledAndDisabled()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage());
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, async (handler) =>
			{
				var rootNavView = (handler.PlatformView);
				Assert.False(rootNavView.IsBackEnabled);
				await shell.Navigation.PushAsync(new ContentPage());
				Assert.True(rootNavView.IsBackEnabled);
				Shell.SetBackButtonBehavior(shell.CurrentPage,
					new BackButtonBehavior()
					{
						IsEnabled = false
					});
				Assert.False(rootNavView.IsBackEnabled);
			});
		}

		[Fact(DisplayName = "Shell with Single Content Page Has Panes Disabled")]
		public async Task BasicShellHasPaneDisplayModeDisabled()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
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
			var shell = await CreateShellAsync((shell) =>
			{
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
			var shell = await CreateShellAsync((shell) =>
			{
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

		[Fact(DisplayName = "Flyout Locked Offset from AppTitleBar")]
		public async Task FlyoutLockedOffsetFromAppTitleBar()
		{
			SetupBuilder();
			var label = new StackLayout()
			{
				HeightRequest = 10
			};

			var shell = await InvokeOnMainThreadAsync(() =>
			{
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
				var distance = rootManager.AppTitleBar.ActualHeight - position.Value.Y;
				Assert.True(Math.Abs(distance) < 1);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Flyout Locked Offsets Correctly from Content")]
		public async Task FlyoutLockedOffsetsCorrectlyFromContent()
		{
			SetupBuilder();
			var flyoutContent = new VerticalStackLayout()
			{
				HorizontalOptions = LayoutOptions.Fill
			};

			var contentPage = new ContentPage()
			{
				Content = new VerticalStackLayout()
			};

			var shell = await InvokeOnMainThreadAsync(() =>
			{
				return new Shell()
				{
					FlyoutContent = flyoutContent,
					CurrentItem = contentPage,
					FlyoutBehavior = FlyoutBehavior.Locked
				};
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				var position = contentPage.GetLocationRelativeTo(flyoutContent);

				var distance = position.Value.X - flyoutContent.Width;
				Assert.True(Math.Abs(distance) < 1);
				return Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "Content Offsets Correctly From App Title Bar")]
		public async Task ContentOffsetsCorrectlyFromAppTitleBar()
		{
			SetupBuilder();
			var flyoutContent = new VerticalStackLayout()
			{
				HorizontalOptions = LayoutOptions.Fill
			};

			var contentPage = new ContentPage()
			{
				Content = new VerticalStackLayout()
			};

			var shell = await InvokeOnMainThreadAsync(() =>
			{
				return new Shell()
				{
					FlyoutContent = flyoutContent,
					CurrentItem = contentPage,
					FlyoutBehavior = FlyoutBehavior.Locked
				};
			});

			await CreateHandlerAndAddToWindow<ShellHandler>(shell, (handler) =>
			{
				var rootManager = handler.MauiContext.GetNavigationRootManager();

				var position = contentPage.GetLocationRelativeTo(rootManager.AppTitleBar);
				var distance = rootManager.AppTitleBar.ActualHeight - position.Value.Y;
				Assert.True(Math.Abs(distance) < 1);
				return Task.CompletedTask;
			});
		}
	}
}
