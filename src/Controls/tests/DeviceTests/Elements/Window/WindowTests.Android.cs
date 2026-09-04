using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowTests
	{
		[Fact]
		public async Task WindowDestroyingPreservesWindowScopeOnAndroid()
		{
			// https://github.com/dotnet/maui/issues/33597
			SetupBuilder();

			var window = new Window(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(window.Page);

				var mauiContext = handler.MauiContext as MauiContext;
				Assert.NotNull(mauiContext);

				var windowScopeField = typeof(MauiContext).GetField("_windowScope", BindingFlags.NonPublic | BindingFlags.Instance);
				var setWindowScope = typeof(MauiContext).GetMethod("SetWindowScope", BindingFlags.NonPublic | BindingFlags.Instance);

				var newScope = mauiContext.Services.CreateScope();
				setWindowScope.Invoke(mauiContext, new[] { newScope });
				Assert.NotNull(windowScopeField.GetValue(mauiContext));

				((IWindow)window).Destroying();

				Assert.NotNull(windowScopeField.GetValue(mauiContext));
			});
		}

		[Fact]
		public async Task WindowDestroyingKeepsLastWindowOnAndroid()
		{
			// https://github.com/dotnet/maui/issues/38020
			// On Android, pressing back destroys the Activity (and thus the window) but the
			// process stays alive. Reopening the app must re-attach the existing window
			// instead of recreating it, otherwise all UI state is lost.
			SetupBuilder();

			var window = new Window(new ContentPage());

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, async handler =>
			{
				await OnLoadedAsync(window.Page);

				var application = MauiContext.Services.GetService<IApplication>();
				Assert.NotNull(application);
				Assert.Contains(window, application.Windows);

				// Simulate the user pressing back (back-to-home), which destroys the window.
				((IWindow)window).Destroying();
				Assert.True(window.IsDestroyed);

				// The last window is kept around so it can be re-attached on relaunch.
				Assert.Contains(window, application.Windows);

				// Relaunch should reuse the existing destroyed window instead of creating a new one.
				var relaunchedWindow = application.CreateWindow(null);
				Assert.Same(window, relaunchedWindow);
			});
		}
	}
}
