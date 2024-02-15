using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using Windows.ApplicationModel.Core;
using ThreadPool = Windows.System.Threading.ThreadPool;

namespace Microsoft.Maui.DeviceTests
{
	public class IdleSynchronizer : IDisposable
	{
		const int _idleTimeoutMs = 100000;

		const string _animationsCompleteHandleName = "AnimationsComplete";
		const string _hasAnimationsHandleName = "HasAnimations";
		const string _imageDecodeIdleHandleName = "ImageDecodingIdle";
		const string _rootVisualResetHandleName = "RootVisualReset";

		readonly DispatcherQueue _dispatcherQueue;

		EventWaitHandle _animationsCompleteHandle;
		EventWaitHandle _hasAnimationsHandle;
		EventWaitHandle _imageDecodeIdleHandle;
		EventWaitHandle _rootVisualResetHandle;

		private static Dictionary<uint, IdleSynchronizer> _idleSynchronizers = new();
		public static IdleSynchronizer GetForCurrentProcess(DispatcherQueue? dispatcherQueue = null)
		{
			var processId = NativeMethods.GetCurrentProcessId();
			if (_idleSynchronizers.TryGetValue(processId, out var idleSynchronizer))
			{
				return idleSynchronizer;
			}
			else
			{
				var newIdleSynchronizer = new IdleSynchronizer(dispatcherQueue ?? DispatcherQueue.GetForCurrentThread());
				_idleSynchronizers[processId] = newIdleSynchronizer;

				return newIdleSynchronizer;
			}
		}

		internal IdleSynchronizer(DispatcherQueue dispatcherQueue)
		{
			_dispatcherQueue = dispatcherQueue ?? throw new ArgumentNullException();

			var processId = NativeMethods.GetCurrentProcessId();
			var threadId = NativeMethods.GetCurrentThreadId();

			_animationsCompleteHandle = OpenNamedEventHandle(processId, threadId, _animationsCompleteHandleName);
			_hasAnimationsHandle = OpenNamedEventHandle(processId, threadId, _hasAnimationsHandleName);
			_imageDecodeIdleHandle = OpenNamedEventHandle(processId, threadId, _imageDecodeIdleHandleName);
			_rootVisualResetHandle = OpenNamedEventHandle(processId, threadId, _rootVisualResetHandleName);
		}

		public void Dispose()
		{
			_hasAnimationsHandle.Dispose();
			_animationsCompleteHandle.Dispose();
			_rootVisualResetHandle.Dispose();

			var processId = NativeMethods.GetCurrentProcessId();
			_idleSynchronizers.Remove(processId);
		}

		public async Task<string> WaitAsync()
		{
			string error = string.Empty;
			await ThreadPool.RunAsync((_) =>
			{
				error = Wait();
			});

			return error;
		}

		public string Wait()
		{
			var errorString = string.Empty;

			bool isIdle = false;
			while (!isIdle)
			{
				errorString = WaitForRootVisualReset();
				if (errorString.Length > 0)
				{ 
					return errorString; 
				}

				errorString = WaitForImageDecodeIdle();
				if (errorString.Length > 0)
				{
					return errorString;
				}

				SynchronouslyTickUIThread(1);
				WaitForIdleDispatcher();

				errorString = WaitForAnimationsComplete(out var hadAnimations);
				if (errorString.Length > 0)
				{ 
					return errorString; 
				}

				isIdle = !hadAnimations;
			}

			return errorString;
		}

		private string WaitForRootVisualReset()
		{
			if (!_rootVisualResetHandle.WaitOne(5000))
			{
				return "Waiting for root visual reset handle returned an invalid value.";
			}
			return string.Empty;
		}

		private string WaitForImageDecodeIdle()
		{
			if (!_imageDecodeIdleHandle.WaitOne(5000))
			{
				return "Waiting for image decoding idle handle returned an invalid value.";
			}
			return string.Empty;
		}

		private void WaitForIdleDispatcher()
		{
			AutoResetEvent shouldContinueEvent = new AutoResetEvent(false);
			_dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
			{
				shouldContinueEvent.Set();
			});
			shouldContinueEvent.WaitOne(10000);
		}

		private string WaitForAnimationsComplete(out bool hadAnimations)
		{
			hadAnimations = false;

			if (!_animationsCompleteHandle.Reset())
			{
				return "Failed to reset AnimationsComplete handle.";
			}

			// This will be signaled if and only if XAML plans to at some point in the near
			// future set the animations complete event.
			uint waitResult = NativeMethods.WaitForSingleObject(
				_hasAnimationsHandle.GetSafeWaitHandle().DangerousGetHandle(), 0);

			if (waitResult != NativeMethods.WAIT_OBJECT_0 && waitResult != NativeMethods.WAIT_TIMEOUT)
			{
				return "HasAnimations handle wait returned an invalid value.";
			}

			bool hasAnimations = (waitResult == NativeMethods.WAIT_OBJECT_0);
			if (hasAnimations)
			{
				if (!_animationsCompleteHandle.WaitOne(_idleTimeoutMs))
				{
					return "Animation complete wait took longer than idle timeout.";
				}
			}

			hadAnimations = hasAnimations;
			return string.Empty;
		}

		private void SynchronouslyTickUIThread(uint ticks)
		{
			AutoResetEvent tickCompleteEvent = new AutoResetEvent(false);

			for (uint i = 0; i < ticks; i++)
			{
				_dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
				{
					EventHandler<object>? renderingHandler = null;
					renderingHandler = (_, _) =>
					{
						CompositionTarget.Rendering -= renderingHandler;
						tickCompleteEvent.Set();
					};

					CompositionTarget.Rendering += renderingHandler;
				});

				tickCompleteEvent.WaitOne();
			}
		}

		private static EventWaitHandle OpenNamedEventHandle(uint processId, uint threadId, string eventNamePrefix)
		{
			EventWaitHandle? eventHandle = null;
			string eventName = $"{eventNamePrefix}.{processId}.{threadId}";

			if (!EventWaitHandle.TryOpenExisting(eventName, out eventHandle))
			{
				eventHandle = EventWaitHandle.OpenExisting(eventNamePrefix);
			}
			
			if (eventHandle == null || eventHandle.SafeWaitHandle.IsInvalid)
			{
				throw new Exception($"Failed to open {eventName} handle.");
			}
			return eventHandle;
		}

		internal static class NativeMethods
		{
			[DllImport("kernel32.dll", SetLastError = true)]
			public static extern uint WaitForSingleObject(IntPtr hHandle, uint dwMilliseconds);

			public const uint INFINITE = 0xFFFFFFFF;
			public const uint WAIT_ABANDONED = 0x00000080;
			public const uint WAIT_OBJECT_0 = 0x00000000;
			public const uint WAIT_TIMEOUT = 0x00000102;

			[DllImport("kernel32.dll")]
			public static extern uint GetCurrentProcessId();

			[DllImport("kernel32.dll")]
			public static extern uint GetCurrentThreadId();
		}
	}
}
