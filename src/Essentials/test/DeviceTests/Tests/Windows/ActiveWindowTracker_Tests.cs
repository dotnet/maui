#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests.Shared
{
	[Category("Windows ActiveWindowTracker")]
	public class Windows_ActiveWindowTracker_Tests : BaseWindowMessageManager_Tests
	{
		[Fact]
		public Task StoppedTrackerDoesNotTrack() =>
			Utils.OnMainThread(async () =>
			{
				var messages = new List<uint>();

				var window = new UI.Xaml.Window();

				var wsm = new TestWindowStateManager();
				var tracker = new ActiveWindowTracker(wsm);
				tracker.WindowMessage += OnWindowMessage;

				wsm.OnActivated(window);
				tracker.Start();
				tracker.Stop();

				await PostTestMessageAsync(window);

				Assert.DoesNotContain(TEST_MESSAGE, messages);

				void OnWindowMessage(object? sender, WindowMessageEventArgs e)
				{
					messages.Add(e.MessageId);
				}
			});

		[Fact]
		public Task ActivatedBeforeStartRecievesMessage() =>
			Utils.OnMainThread(async () =>
			{
				var messages = new List<uint>();

				var window = new UI.Xaml.Window();

				var wsm = new TestWindowStateManager();
				var tracker = new ActiveWindowTracker(wsm);
				tracker.WindowMessage += OnWindowMessage;

				wsm.OnActivated(window);
				tracker.Start();

				await PostTestMessageAsync(window);

				tracker.Stop();

				Assert.Contains(TEST_MESSAGE, messages);

				void OnWindowMessage(object? sender, WindowMessageEventArgs e)
				{
					messages.Add(e.MessageId);
				}
			});

		[Fact]
		public Task StartBeforeActivatedRecievesMessage() =>
			Utils.OnMainThread(async () =>
			{
				var messages = new List<uint>();

				var window = new UI.Xaml.Window();

				var wsm = new TestWindowStateManager();
				wsm.OnActivated(window);

				var tracker = new ActiveWindowTracker(wsm);
				tracker.Start();
				tracker.WindowMessage += OnWindowMessage;

				await PostTestMessageAsync(window);

				Assert.Contains(TEST_MESSAGE, messages);

				void OnWindowMessage(object? sender, WindowMessageEventArgs e)
				{
					messages.Add(e.MessageId);
				}
			});

		[Fact(Skip = "Window message handling does not work reliably in headless/CI environments")]
		public Task SwitchingWindowsPostsToTheNewWindow() =>
			Utils.OnMainThread(async () =>
			{
				var messages = new List<(IntPtr, uint)>();

				var window1 = new UI.Xaml.Window();
				var window2 = new UI.Xaml.Window();

				var wsm = new TestWindowStateManager();
				var tracker = new ActiveWindowTracker(wsm);
				tracker.Start();
				tracker.WindowMessage += OnWindowMessage;

				wsm.OnActivated(window1);
				wsm.OnActivated(window2);

				await PostTestMessageAsync(window2);

				Assert.DoesNotContain((window1.GetWindowHandle(), TEST_MESSAGE), messages);
				Assert.Contains((window2.GetWindowHandle(), TEST_MESSAGE), messages);

				void OnWindowMessage(object? sender, WindowMessageEventArgs e)
				{
					messages.Add((e.Hwnd, e.MessageId));
				}
			});

		[Fact]
		public Task SwitchingWindowsDoesNotPostToTheOldWindow() =>
			Utils.OnMainThread(async () =>
			{
				var messages = new List<(IntPtr, uint)>();

				var window1 = new UI.Xaml.Window();
				var window2 = new UI.Xaml.Window();

				var wsm = new TestWindowStateManager();
				var tracker = new ActiveWindowTracker(wsm);
				tracker.Start();
				tracker.WindowMessage += OnWindowMessage;

				wsm.OnActivated(window1);
				wsm.OnActivated(window2);

				await PostTestMessageAsync(window1);

				Assert.DoesNotContain((window1.GetWindowHandle(), TEST_MESSAGE), messages);
				Assert.DoesNotContain((window2.GetWindowHandle(), TEST_MESSAGE), messages);

				void OnWindowMessage(object? sender, WindowMessageEventArgs e)
				{
					messages.Add((e.Hwnd, e.MessageId));
				}
			});

		class TestWindowStateManager : IWindowStateManager
		{
			UI.Xaml.Window? _window;

			public event EventHandler? ActiveWindowChanged;

			public UI.Xaml.Window? GetActiveWindow() => _window;

			public void OnActivated(UI.Xaml.Window window, UI.Xaml.WindowActivatedEventArgs? args = null)
			{
				SetActiveWindow(window);
			}

			public void OnPlatformWindowInitialized(UI.Xaml.Window window)
			{
				SetActiveWindow(window);
			}

			void SetActiveWindow(UI.Xaml.Window window)
			{
				if (_window != window)
				{
					_window = window;
					ActiveWindowChanged?.Invoke(window, EventArgs.Empty);
				}
			}
		}
	}
}
