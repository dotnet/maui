using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests.Shared
{
	[Category("Windows WindowMessageManager")]
	public class Windows_WindowMessageManager_Tests
	{
		[Fact]
		public Task NewWindowIsNotFound() =>
			Utils.OnMainThread(() =>
			{
				var window = new UI.Xaml.Window();
				var handle = window.GetWindowHandle();

				var allManagers = WindowMessageManager.GetAll().ToArray();
				var allHandles = allManagers.Select(m => m.WindowHandle).ToArray();

				Assert.DoesNotContain(handle, allHandles);
			});

		[Fact]
		public Task RegisteredWindowIsFound() =>
			Utils.OnMainThread(() =>
			{
				var window = new UI.Xaml.Window();
				var handle = window.GetWindowHandle();

				var manager = WindowMessageManager.Get(window);

				var allManagers = WindowMessageManager.GetAll().ToArray();
				var allHandles = allManagers.Select(m => m.WindowHandle).ToArray();

				Assert.Contains(handle, allHandles);
				Assert.Contains(manager, allManagers);
			});

		[Fact]
		public Task DisposedManagerIsNotFound() =>
			Utils.OnMainThread(() =>
			{
				var window = new UI.Xaml.Window();
				var handle = window.GetWindowHandle();

				var manager = WindowMessageManager.Get(window);

				manager.Dispose();

				var allManagers = WindowMessageManager.GetAll().ToArray();
				var allHandles = allManagers.Select(m => m.WindowHandle).ToArray();

				Assert.DoesNotContain(handle, allHandles);
				Assert.DoesNotContain(manager, allManagers);
			});

		[Fact]
		public Task PostingMessagesReachesEvent() =>
			Utils.OnMainThread(async () =>
			{
				const uint message = WM_APP + 1;
				var messages = new List<uint>();

				var window = new UI.Xaml.Window();
				var handle = window.GetWindowHandle();

				var manager = WindowMessageManager.Get(window);
				manager.WindowMessage += OnWindowMessage;

				PostMessage(handle, message, IntPtr.Zero, IntPtr.Zero);

				await Task.Delay(100);

				Assert.Contains(message, messages);

				void OnWindowMessage(object sender, WindowMessageEventArgs e)
				{
					messages.Add(e.MessageId);
				}
			});

		[Fact]
		public Task PostingMessagesToDisposedManagerDoesNotReachEvent() =>
			Utils.OnMainThread(async () =>
			{
				const uint message = WM_APP + 1;
				var messages = new List<uint>();

				var window = new UI.Xaml.Window();
				var handle = window.GetWindowHandle();

				var manager = WindowMessageManager.Get(window);
				manager.WindowMessage += OnWindowMessage;
				manager.Dispose();

				PostMessage(handle, message, IntPtr.Zero, IntPtr.Zero);

				await Task.Delay(100);

				Assert.DoesNotContain(message, messages);

				void OnWindowMessage(object sender, WindowMessageEventArgs e)
				{
					messages.Add(e.MessageId);
				}
			});

		const uint WM_APP = 0x8000;

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
	}
}
