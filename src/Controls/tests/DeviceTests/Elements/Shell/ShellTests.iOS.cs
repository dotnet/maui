#if !IOS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ShellTests : HandlerTestBase
	{
		protected Task CheckFlyoutState(ShellRenderer renderer, bool result) =>
			throw new NotImplementedException();

		[Fact(DisplayName = "Swiping Away Modal Propagates to Shell")]
		public async Task SwipingAwayModalPropagatesToShell()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage());
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				var modalPage = new ContentPage();
				modalPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
				var platformWindow = MauiContext.GetPlatformWindow().RootViewController;

				await shell.Navigation.PushModalAsync(modalPage);

				var modalVC = GetModalWrapper(modalPage);
				int navigatedFired = 0;
				ShellNavigationSource? shellNavigationSource = null;
				var finishedNavigation = new TaskCompletionSource<bool>();
				shell.Navigated += ShellNavigated;

				modalVC.DidDismiss(null);
				await finishedNavigation.Task.WaitAsync(TimeSpan.FromSeconds(2));
				Assert.Equal(1, navigatedFired);
				Assert.Equal(ShellNavigationSource.PopToRoot, shellNavigationSource.Value);

				Assert.Equal(0, shell.Navigation.ModalStack.Count);

				void ShellNavigated(object sender, ShellNavigatedEventArgs e)
				{
					navigatedFired++;
					shellNavigationSource = e.Source;
					finishedNavigation.SetResult(true);
				}
			});
		}

		[Fact(DisplayName = "Swiping Away Modal Removes Entire Navigation Page")]
		public async Task SwipingAwayModalRemovesEntireNavigationPage()
		{
			Routing.RegisterRoute(nameof(SwipingAwayModalRemovesEntireNavigationPage), typeof(ModalShellPage));

			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage());
			});

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				var modalPage = new Controls.NavigationPage(new ContentPage());
				modalPage.On<iOS>().SetModalPresentationStyle(UIModalPresentationStyle.FormSheet);
				var platformWindow = MauiContext.GetPlatformWindow().RootViewController;

				await shell.Navigation.PushModalAsync(modalPage);
				await shell.GoToAsync(nameof(SwipingAwayModalRemovesEntireNavigationPage));
				await shell.GoToAsync(nameof(SwipingAwayModalRemovesEntireNavigationPage));
				await shell.GoToAsync(nameof(SwipingAwayModalRemovesEntireNavigationPage));

				var modalVC = GetModalWrapper(modalPage);
				int navigatedFired = 0;
				ShellNavigationSource? shellNavigationSource = null;
				var finishedNavigation = new TaskCompletionSource<bool>();
				shell.Navigated += ShellNavigated;

				modalVC.DidDismiss(null);
				await finishedNavigation.Task.WaitAsync(TimeSpan.FromSeconds(2));
				Assert.Equal(1, navigatedFired);
				Assert.Equal(ShellNavigationSource.PopToRoot, shellNavigationSource.Value);
				Assert.Equal(0, shell.Navigation.ModalStack.Count);

				void ShellNavigated(object sender, ShellNavigatedEventArgs e)
				{
					navigatedFired++;
					shellNavigationSource = e.Source;
					finishedNavigation.SetResult(true);
				}
			});
		}

		class ModalShellPage : ContentPage
		{
			public ModalShellPage()
			{
				Shell.SetPresentationMode(this, PresentationMode.ModalAnimated);
			}
		}
	}
}
#endif