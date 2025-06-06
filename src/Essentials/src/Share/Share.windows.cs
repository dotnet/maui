using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using WinRT;

namespace Microsoft.Maui.ApplicationModel.DataTransfer
{
	partial class ShareImplementation : IShare
	{
		Task PlatformRequestAsync(ShareTextRequest request)
		{
			var hwnd = WindowStateManager.Default.GetActiveWindowHandle(true);
			var dataTransferManager = DataTransferManagerHelper.GetDataTransferManager(hwnd);

			dataTransferManager.DataRequested += ShareTextHandler;

			DataTransferManagerHelper.ShowShare(hwnd);

			void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
			{
				var newRequest = e.Request;

				newRequest.Data.Properties.Title = request.Title ?? AppInfo.Current.Name;

				if (!string.IsNullOrWhiteSpace(request.Text))
				{
					newRequest.Data.SetText(request.Text);
				}

				if (!string.IsNullOrWhiteSpace(request.Uri))
				{
					newRequest.Data.SetWebLink(new Uri(request.Uri));
				}

				dataTransferManager.DataRequested -= ShareTextHandler;
			}

			return Task.CompletedTask;
		}

		Task PlatformRequestAsync(ShareFileRequest request) =>
			PlatformRequestAsync((ShareMultipleFilesRequest)request);

		async Task PlatformRequestAsync(ShareMultipleFilesRequest request)
		{
			var storageFiles = new List<IStorageFile>();
			foreach (var file in request.Files)
				storageFiles.Add(file.File ?? await StorageFile.GetFileFromPathAsync(file.FullPath));

			var hwnd = WindowStateManager.Default.GetActiveWindowHandle(true);
			var dataTransferManager = DataTransferManagerHelper.GetDataTransferManager(hwnd);

			dataTransferManager.DataRequested += ShareTextHandler;

			DataTransferManagerHelper.ShowShare(hwnd);

			void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
			{
				var newRequest = e.Request;

				newRequest.Data.SetStorageItems(storageFiles.ToArray());
				newRequest.Data.Properties.Title = request.Title ?? AppInfo.Current.Name;

				dataTransferManager.DataRequested -= ShareTextHandler;
			}
		}
	}

	static class DataTransferManagerHelper
	{
		[ComImport]
		[Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		public interface IDataTransferManagerInterop
		{
			IntPtr GetForWindow([In] IntPtr appWindow, [In] ref Guid riid);
			void ShowShareUIForWindow(IntPtr appWindow);
		}

		public static DataTransferManager GetDataTransferManager(IntPtr appWindow)
		{
			var interop = DataTransferManager.As<IDataTransferManagerInterop>();
			Guid id = new Guid(0xa5caee9b, 0x8708, 0x49d1, 0x8d, 0x36, 0x67, 0xd2, 0x5a, 0x8d, 0xa0, 0x0c);
			IntPtr result;
			result = interop.GetForWindow(appWindow, id);
			DataTransferManager dataTransferManager = MarshalInterface<DataTransferManager>.FromAbi(result);
			return (dataTransferManager);
		}

		public static void ShowShare(IntPtr appWindow)
		{
			var interop = DataTransferManager.As<IDataTransferManagerInterop>();
			interop.ShowShareUIForWindow(appWindow);
		}
	}
}
