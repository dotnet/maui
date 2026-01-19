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

		[Fact]
		public Task SwitchingWindowsPostsToTheNewWindow() =>
			Utils.OnMainThread(async () =>
			{
				var messages = new List<(IntPtr, uint)>();

				var window1 = new UI.Xaml.Window();
				var window2 = new UI.Xaml.Window();

				System.Diagnostics.Debug.WriteLine($"[TEST] window1 handle: {window1.GetWindowHandle()}");
				System.Diagnostics.Debug.WriteLine($"[TEST] window2 handle: {window2.GetWindowHandle()}");

				var wsm = new TestWindowStateManager();
				var tracker = new ActiveWindowTracker(wsm);
				tracker.Start();
				tracker.WindowMessage += OnWindowMessage;

				wsm.OnActivated(window1);
				System.Diagnostics.Debug.WriteLine($"[TEST] Activated window1, active window: {wsm.GetActiveWindow()?.GetWindowHandle()}");
				wsm.OnActivated(window2);
				System.Diagnostics.Debug.WriteLine($"[TEST] Activated window2, active window: {wsm.GetActiveWindow()?.GetWindowHandle()}");

				await PostTestMessageAsync(window2);
				System.Diagnostics.Debug.WriteLine($"[TEST] Posted message to window2, received {messages.Count} messages");
				foreach (var msg in messages)
				{
					System.Diagnostics.Debug.WriteLine($"[TEST]   Message: hwnd={msg.Item1}, msgId={msg.Item2}");
				}

				Assert.DoesNotContain((window1.GetWindowHandle(), TEST_MESSAGE), messages);
				Assert.Contains((window2.GetWindowHandle(), TEST_MESSAGE), messages);

				void OnWindowMessage(object? sender, WindowMessageEventArgs e)
				{
					System.Diagnostics.Debug.WriteLine($"[TEST] OnWindowMessage: hwnd={e.Hwnd}, msgId={e.MessageId}");
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
