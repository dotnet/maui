#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.UI.Xaml;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests.Shared
{
	[Category("Windows DeviceDisplay")]
	public class DeviceDisplay_Windows_Tests
	{
		[Fact]
		public Task Screen_Metrics_Are_Valid_Before_Window_Activated()
		{
			return Utils.OnMainThread(() =>
			{
				var oldStateManager = WindowStateManager.Default;

				TestWindowStateManagerImplementation testWindowStateManager = new();
				WindowStateManager.SetDefault(testWindowStateManager);

				// Create the window but don't activate it
				var window = new MauiWinUIWindow();
				var metrics = DeviceDisplay.MainDisplayInfo;

				WindowStateManager.SetDefault(oldStateManager);

				Assert.NotNull(testWindowStateManager.GetPlatformInitializedWindow());
				Assert.NotNull(testWindowStateManager.GetActiveWindow());
				Assert.True(metrics.Width > 0);
				Assert.True(metrics.Height > 0);
				Assert.True(metrics.Density > 0);
			});
		}

		class TestWindowStateManagerImplementation : IWindowStateManager
		{
			Window? _activeWindow;
			Window? _activePlatformSetWindow;

			public event EventHandler? ActiveWindowChanged;

			public Window? GetActiveWindow() =>
				_activeWindow;

			public Window? GetPlatformInitializedWindow() =>
				_activePlatformSetWindow;

			void SetActiveWindow(Window window)
			{
				if (_activeWindow == window)
					return;

				_activeWindow = window;

				ActiveWindowChanged?.Invoke(window, EventArgs.Empty);
			}

			public void OnPlatformWindowInitialized(Window window)
			{
				_activePlatformSetWindow = window;
				SetActiveWindow(window);
			}

			public void OnActivated(Window window, WindowActivatedEventArgs args)
			{
				if (args.WindowActivationState != WindowActivationState.Deactivated)
					SetActiveWindow(window);
			}
		}
	}
}
