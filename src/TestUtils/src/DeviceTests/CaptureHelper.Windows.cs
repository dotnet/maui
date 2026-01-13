using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml;
using Windows.Graphics.Capture;
using Windows.Graphics.DirectX;

namespace Microsoft.Maui.DeviceTests
{
	public static class CaptureHelper
	{
		static readonly Guid GraphicsCaptureItemGuid = new("79C3F95B-31F7-4EC2-A464-632EF5D30760");

		/// <summary>
		/// Set this to a directory path where logs should be written.
		/// On Helix, this should be set to the directory of the test results file.
		/// </summary>
		public static string? LogDirectory { get; set; }

		// Log to a file that will be uploaded as a Helix artifact
		static void Log(string message)
		{
			var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
			var logMessage = $"[{timestamp}] [CaptureHelper] {message}";
			Console.WriteLine(logMessage);
			
			try
			{
				// Priority order for log directory:
				// 1. LogDirectory if explicitly set (set by test runner from test results file path)
				// 2. HELIX_WORKITEM_UPLOAD_ROOT environment variable
				// 3. Temp directory as fallback
				var logDir = LogDirectory;
				if (string.IsNullOrEmpty(logDir))
				{
					logDir = Environment.GetEnvironmentVariable("HELIX_WORKITEM_UPLOAD_ROOT");
				}
				if (string.IsNullOrEmpty(logDir))
				{
					logDir = Path.GetTempPath();
				}
				
				var logFile = Path.Combine(logDir, "CaptureHelper.log");
				File.AppendAllText(logFile, logMessage + Environment.NewLine);
			}
			catch
			{
				// Ignore logging failures
			}
		}

		[ComImport]
		[Guid("3628E81B-3CAC-4C60-B7F4-23CE0E0C3356")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		[ComVisible(true)]
		interface IGraphicsCaptureItemInterop
		{
			IntPtr CreateForWindow(
				[In] IntPtr window,
				[In] ref Guid iid);

			IntPtr CreateForMonitor(
				[In] IntPtr monitor,
				[In] ref Guid iid);
		}

		public static GraphicsCaptureItem CreateItemForWindow(IntPtr hwnd)
		{
			var isSupported = GraphicsCaptureSession.IsSupported();
			Log($"CreateItemForWindow called: hwnd=0x{hwnd:X}, IsSupported={isSupported}");

			if (!isSupported)
			{
				Log("IsSupported=false, throwing PlatformNotSupportedException");
				throw new PlatformNotSupportedException(
					"Windows Graphics Capture API is not supported in this environment. " +
					"This commonly occurs on Helix CI VMs where the required graphics infrastructure is not available.");
			}

			try
			{
				var interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
				var itemPointer = interop.CreateForWindow(hwnd, GraphicsCaptureItemGuid);
				var item = GraphicsCaptureItem.FromAbi(itemPointer);
				Marshal.Release(itemPointer);
				Log($"CreateItemForWindow SUCCESS: Size={item.Size}");
				return item;
			}
			catch (ArgumentException ex) when (ex.Message.Contains("Value does not fall within the expected range", StringComparison.Ordinal))
			{
				Log($"CreateItemForWindow FAILED with ArgumentException: {ex.Message}");
				throw new PlatformNotSupportedException(
					"Windows Graphics Capture failed: the display environment does not support window capture. " +
					"This commonly occurs on Helix CI VMs.", ex);
			}
		}

		public static GraphicsCaptureItem CreateItemForMonitor(IntPtr hmon)
		{
			var interop = GraphicsCaptureItem.As<IGraphicsCaptureItemInterop>();
			var itemPointer = interop.CreateForMonitor(hmon, GraphicsCaptureItemGuid);
			var item = GraphicsCaptureItem.FromAbi(itemPointer);
			Marshal.Release(itemPointer);

			return item;
		}

		public static Task<CanvasBitmap> RenderAsync(Window window, CanvasDevice device)
		{
			var handle = WinRT.Interop.WindowNative.GetWindowHandle(window);
			var item = CreateItemForWindow(handle);
			return item.RenderAsync(device);
		}

		public static async Task<CanvasBitmap> RenderAsync(this GraphicsCaptureItem item, CanvasDevice device)
		{
			var tcs = new TaskCompletionSource<CanvasBitmap>();

			using (var framePool = Direct3D11CaptureFramePool.Create(device, DirectXPixelFormat.B8G8R8A8UIntNormalized, 1, item.Size))
			using (var session = framePool.CreateCaptureSession(item))
			{
				framePool.FrameArrived += OnFrameArrived;

				// Start capturing. The FrameArrived event will occur shortly.
				session.StartCapture();

				// Wait for the frame to arrive.
				var result = await tcs.Task;

				// !!!!!!!! NOTE !!!!!!!!
				// This thread is now running inside the OnFrameArrived callback method.

				// Unsubscribe now that we have captured the frame.
				framePool.FrameArrived -= OnFrameArrived;

				// Yield to allow the OnFrameArrived callback to unwind so that it is safe to
				// Dispose the session and framepool.
				await Task.Yield();
			}

			return await tcs.Task;

			void OnFrameArrived(Direct3D11CaptureFramePool sender, object args)
			{
				using var frame = sender.TryGetNextFrame();
				if (frame is null)
					tcs.SetException(new InvalidOperationException("A null frame was recieved."));
				else
					tcs.SetResult(CanvasBitmap.CreateFromDirect3D11Surface(device, frame.Surface));
			}
		}
	}
}
