using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace Microsoft.Maui.Essentials
{
	public static partial class Share
	{
		static Task PlatformRequestAsync(ShareTextRequest request)
		{
			var dataTransferManager = DataTransferManager.GetForCurrentView();

			dataTransferManager.DataRequested += ShareTextHandler;

			DataTransferManager.ShowShareUI();

			void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
			{
				var newRequest = e.Request;

				newRequest.Data.Properties.Title = request.Title ?? AppInfo.Name;

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

		static async Task PlatformRequestAsync(ShareMultipleFilesRequest request)
		{
			var storageFiles = new List<IStorageFile>();
			foreach (var file in request.Files)
				storageFiles.Add(file.File ?? await StorageFile.GetFileFromPathAsync(file.FullPath));

			var dataTransferManager = DataTransferManager.GetForCurrentView();

			dataTransferManager.DataRequested += ShareTextHandler;

			DataTransferManager.ShowShareUI();

			void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
			{
				var newRequest = e.Request;

				newRequest.Data.SetStorageItems(storageFiles.ToArray());
				newRequest.Data.Properties.Title = request.Title ?? AppInfo.Name;

				dataTransferManager.DataRequested -= ShareTextHandler;
			}
		}
	}
}
