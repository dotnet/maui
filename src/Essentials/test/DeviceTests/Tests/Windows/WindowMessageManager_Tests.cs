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
		[UIFact]
		public void NewWindowIsNotFound()
			{
				var window = new UI.Xaml.Window();
				var handle = window.GetWindowHandle();

				var allManagers = WindowMessageManager.GetAll().ToArray();
				var allHandles = allManagers.Select(m => m.WindowHandle).ToArray();

				Assert.DoesNotContain(handle, allHandles);
			}

		[UIFact]
		public void RegisteredWindowIsFound()
			{
				var window = new UI.Xaml.Window();
				var handle = window.GetWindowHandle();

				var manager = WindowMessageManager.Get(window);

				var allManagers = WindowMessageManager.GetAll().ToArray();
				var allHandles = allManagers.Select(m => m.WindowHandle).ToArray();

				manager.Dispose();

				Assert.Contains(handle, allHandles);
				Assert.Contains(manager, allManagers);
			}

		[UIFact]
		public void RegisteredWindowIsNotAttached()
			{
				var window = new UI.Xaml.Window();
				var handle = window.GetWindowHandle();

				var manager = WindowMessageManager.Get(window);

				var isAttached = manager.IsAttached;

				manager.Dispose();

				Assert.False(isAttached);
			}

		[UIFact]
		public void SubscribedWindowIsAttached()
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
			}

		[UIFact]
		public void UnsubscribedWindowIsNotAttached()
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
			}

		[UIFact]
		public void DisposedManagerIsNotFound()
			{
				var window = new UI.Xaml.Window();
				var handle = window.GetWindowHandle();

				var manager = WindowMessageManager.Get(window);

				manager.Dispose();

				var allManagers = WindowMessageManager.GetAll().ToArray();
				var allHandles = allManagers.Select(m => m.WindowHandle).ToArray();

				Assert.DoesNotContain(handle, allHandles);
				Assert.DoesNotContain(manager, allManagers);
			}

		[UIFact]
		public async Task PostingMessagesReachesEvent()
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
			}

		[UIFact]
		public async Task PostingMessagesToDisposedManagerDoesNotReachEvent()
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
			}

		[UIFact]
		public async Task PostingMessagesToUnsubscribedManagerDoesNotReachEvent()
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
			}

		[UIFact]
		public async Task PostingMessagesToMultipleSubscribedManagerReachesEvent()
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
			}
	}
}
