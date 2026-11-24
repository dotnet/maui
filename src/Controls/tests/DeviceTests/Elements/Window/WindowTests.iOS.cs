using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class WindowTests
	{
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task WillTerminateDestroysMainPageWindowOnce(bool sceneDisconnectedFirst)
		{
			SetupBuilder();

			var page = new ContentPage();
			var window = new Window(page);
			var app = (ApplicationStub)ApplicationServices.GetRequiredService<IApplication>();
#pragma warning disable CS0618 // Type or member is obsolete
			await InvokeOnMainThreadAsync(() => app.MainPage = page);
#pragma warning restore CS0618 // Type or member is obsolete

			var destroyingCount = 0;
			window.Destroying += (_, _) => destroyingCount++;

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(window, _ =>
			{
				var originalPlatformApplication = IPlatformApplication.Current;
				var shouldDisconnectKeyboardLifecycle = KeyboardAutoManagerScroll.ShouldDisconnectLifecycle;
				IPlatformApplication.Current = new PlatformApplicationStub(ApplicationServices, app);

				try
				{
					// A force-closed scene app gets sceneDidDisconnect: before applicationWillTerminate:
					if (sceneDisconnectedFirst)
						((IWindow)window).Destroying();

					// The MainPage window stays in Application.Windows after it is destroyed
					Assert.Same(window, app.Windows.Single());

					var exception = Record.Exception(() =>
						ApplicationServices.InvokeLifecycleEvents<iOSLifecycle.WillTerminate>(del => del(UIApplication.SharedApplication)));

					Assert.Null(exception);
					Assert.Equal(1, destroyingCount);
				}
				finally
				{
					IPlatformApplication.Current = originalPlatformApplication;

					// WillTerminate disconnects the keyboard observers; restore them for the tests that follow
					KeyboardAutoManagerScroll.ShouldDisconnectLifecycle = shouldDisconnectKeyboardLifecycle;
					if (!shouldDisconnectKeyboardLifecycle)
						KeyboardAutoManagerScroll.Connect();
				}

				return Task.CompletedTask;
			});
		}

		class PlatformApplicationStub : IPlatformApplication
		{
			public PlatformApplicationStub(IServiceProvider services, IApplication application)
			{
				Services = services;
				Application = application;
			}

			public IServiceProvider Services { get; }

			public IApplication Application { get; }
		}
	}
}
