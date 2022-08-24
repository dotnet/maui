#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests.Shared
{
	[Category("Windows WindowMessageManager")]
	public class Windows_WindowMessageManager_Tests : BaseWindowMessageManager_Tests
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

				manager.Dispose();

				Assert.Contains(handle, allHandles);
				Assert.Contains(manager, allManagers);
			});

		[Fact]
		public Task RegisteredWindowIsNotAttached() =>
			Utils.OnMainThread(() =>
			{
				var window = new UI.Xaml.Window();
				var handle = window.GetWindowHandle();

				var manager = WindowMessageManager.Get(window);

				var isAttached = manager.IsAttached;

				manager.Dispose();

				Assert.False(isAttached);
			});

		[Fact]
		public Task SubscribedWindowIsAttached() =>
			Utils.OnMainThread(() =>
			{
				var window = new UI.Xaml.Window();
				var handle = window.GetWindowHandle();

				var manager = WindowMessageManager.Get(window);
				manager.WindowMessage += OnWindowMessage;

				var isAttached = manager.IsAttached;

				manager.Dispose();

				Assert.True(isAttached);

				static void OnWindowMessage(object? sender, WindowMessageEventArgs e)
				{
				}
			});

		[Fact]
		public Task UnsubscribedWindowIsNotAttached() =>
			Utils.OnMainThread(() =>
			{
				var window = new UI.Xaml.Window();
				var handle = window.GetWindowHandle();

				var manager = WindowMessageManager.Get(window);
				manager.WindowMessage += OnWindowMessage;
				manager.WindowMessage -= OnWindowMessage;

				var isAttached = manager.IsAttached;

				manager.Dispose();

				Assert.False(isAttached);

				static void OnWindowMessage(object? sender, WindowMessageEventArgs e)
				{
				}
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
				var messages = new List<uint>();

				var window = new UI.Xaml.Window();

				var manager = WindowMessageManager.Get(window);
				manager.WindowMessage += OnWindowMessage;

				await PostTestMessageAsync(window);

				manager.Dispose();

				Assert.Contains(TEST_MESSAGE, messages);

				void OnWindowMessage(object? sender, WindowMessageEventArgs e)
				{
					messages.Add(e.MessageId);
				}
			});

		[Fact]
		public Task PostingMessagesToDisposedManagerDoesNotReachEvent() =>
			Utils.OnMainThread(async () =>
			{
				var messages = new List<uint>();

				var window = new UI.Xaml.Window();

				var manager = WindowMessageManager.Get(window);
				manager.WindowMessage += OnWindowMessage;
				manager.Dispose();

				await PostTestMessageAsync(window);

				Assert.DoesNotContain(TEST_MESSAGE, messages);

				void OnWindowMessage(object? sender, WindowMessageEventArgs e)
				{
					messages.Add(e.MessageId);
				}
			});

		[Fact]
		public Task PostingMessagesToUnsubscribedManagerDoesNotReachEvent() =>
			Utils.OnMainThread(async () =>
			{
				var messages = new List<uint>();

				var window = new UI.Xaml.Window();

				using var manager = WindowMessageManager.Get(window);
				manager.WindowMessage += OnWindowMessage;
				manager.WindowMessage -= OnWindowMessage;

				await PostTestMessageAsync(window);

				manager.Dispose();

				Assert.DoesNotContain(TEST_MESSAGE, messages);

				void OnWindowMessage(object? sender, WindowMessageEventArgs e)
				{
					messages.Add(e.MessageId);
				}
			});

		[Fact]
		public Task PostingMessagesToMultipleSubscribedManagerReachesEvent() =>
			Utils.OnMainThread(async () =>
			{
				var messages = new List<uint>();

				var window = new UI.Xaml.Window();

				using var manager = WindowMessageManager.Get(window);
				manager.WindowMessage += OnWindowMessage;
				manager.WindowMessage += OnWindowMessage;
				manager.WindowMessage -= OnWindowMessage;

				await PostTestMessageAsync(window);

				manager.Dispose();

				Assert.Contains(TEST_MESSAGE, messages);

				void OnWindowMessage(object? sender, WindowMessageEventArgs e)
				{
					messages.Add(e.MessageId);
				}
			});
	}
}
