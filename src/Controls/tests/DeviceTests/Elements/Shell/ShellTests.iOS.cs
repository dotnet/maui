#if !IOS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Shell)]
	public partial class ShellTests
	{
		protected Task CheckFlyoutState(ShellRenderer renderer, bool result) =>
			throw new NotImplementedException();


		[Fact(DisplayName = "Clicking BackButton Fires Correct Navigation Events")]
		public async Task ShellWithFlyoutDisabledDoesntRenderFlyout()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage());
			});


			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				var secondPage = new ContentPage();
				await shell.Navigation.PushAsync(new ContentPage())
					.WaitAsync(TimeSpan.FromSeconds(2));

				IShellContext shellContext = handler;
				var sectionRenderer = (shellContext.CurrentShellItemRenderer as ShellItemRenderer)
					.CurrentRenderer as ShellSectionRenderer;

				int navigatingFired = 0;
				int navigatedFired = 0;
				var finishedNavigation = new TaskCompletionSource<bool>();
				ShellNavigationSource? shellNavigationSource = null;

				shell.Navigating += ShellNavigating;
				shell.Navigated += ShellNavigated;
				sectionRenderer.SendPop();
				await finishedNavigation.Task.WaitAsync(TimeSpan.FromSeconds(2));
				Assert.Equal(1, navigatingFired);
				Assert.Equal(1, navigatedFired);
				Assert.Equal(ShellNavigationSource.PopToRoot, shellNavigationSource.Value);

				void ShellNavigated(object sender, ShellNavigatedEventArgs e)
				{
					navigatedFired++;
					shellNavigationSource = e.Source;
					finishedNavigation.SetResult(true);
				}

				void ShellNavigating(object sender, ShellNavigatingEventArgs e)
				{
					navigatingFired++;
				}
			});
		}

		[Fact(DisplayName = "Cancel BackButton Navigation")]
		public async Task CancelBackButtonNavigation()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage());
			});


			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, async (handler) =>
			{
				var secondPage = new ContentPage();
				await shell.Navigation.PushAsync(new ContentPage())
					.WaitAsync(TimeSpan.FromSeconds(2));

				IShellContext shellContext = handler;
				var sectionRenderer = (shellContext.CurrentShellItemRenderer as ShellItemRenderer)
					.CurrentRenderer as ShellSectionRenderer;

				int navigatingFired = 0;
				int navigatedFired = 0;
				ShellNavigationSource? shellNavigationSource = null;

				shell.Navigating += ShellNavigating;
				shell.Navigated += ShellNavigated;
				var finishedNavigation = new TaskCompletionSource<bool>();
				sectionRenderer.SendPop();

				// Give Navigated time to fire just in case
				await Task.Delay(100);
				Assert.Equal(1, navigatingFired);
				Assert.Equal(0, navigatedFired);
				Assert.False(shellNavigationSource.HasValue);

				void ShellNavigated(object sender, ShellNavigatedEventArgs e)
				{
					navigatedFired++;
				}

				void ShellNavigating(object sender, ShellNavigatingEventArgs e)
				{
					navigatingFired++;
					e.Cancel();
				}
			});
		}
	}
}
#endif