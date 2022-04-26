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
	[Collection(HandlerTestBase.RunInNewWindowCollection)]
	public partial class ShellTests
	{
		protected async Task CheckFlyoutState(ShellRenderer handler, bool desiredState)
		{
			var drawerLayout = (handler as IShellContext).CurrentDrawerLayout;
			var flyout = drawerLayout.GetChildAt(1);

			if (drawerLayout.IsDrawerOpen(flyout) == desiredState)
			{
				Assert.Equal(desiredState, drawerLayout.IsDrawerOpen(flyout));
				return;
			}

			var taskCompletionSource = new TaskCompletionSource<bool>();
			flyout.LayoutChange += OnLayoutChanged;

			try
			{
				await taskCompletionSource.Task.WaitAsync(TimeSpan.FromSeconds(2));
			}
			catch (TimeoutException)
			{

			}

			flyout.LayoutChange -= OnLayoutChanged;
			Assert.Equal(desiredState, drawerLayout.IsDrawerOpen(flyout));

			return;

			void OnLayoutChanged(object sender, Android.Views.View.LayoutChangeEventArgs e)
			{
				if (drawerLayout.IsDrawerOpen(flyout) == desiredState)
				{
					taskCompletionSource.SetResult(true);
					flyout.LayoutChange -= OnLayoutChanged;
				}
			}
		}

		[Fact(DisplayName = "Shell with Flyout Disabled Doesn't Render Flyout")]
		public async Task ShellWithFlyoutDisabledDoesntRenderFlyout()
		{
			SetupBuilder();
			var shell = await CreateShellAsync((shell) =>
			{
				shell.Items.Add(new ContentPage());
			});

			shell.FlyoutBehavior = FlyoutBehavior.Disabled;

			await CreateHandlerAndAddToWindow<ShellRenderer>(shell, (handler) =>
			{
				IShellContext shellContext = handler;
				var dl = shellContext.CurrentDrawerLayout;
				Assert.Equal(1, dl.ChildCount);
				shell.FlyoutBehavior = FlyoutBehavior.Flyout;
				Assert.Equal(2, dl.ChildCount);
				return Task.CompletedTask;
			});
		}
	}
}
