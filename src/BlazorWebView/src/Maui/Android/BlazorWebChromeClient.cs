
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Webkit;
using Microsoft.Maui.Essentials;
using File = Java.IO.File;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	class BlazorWebChromeClient : WebChromeClient
	{
		public override bool OnCreateWindow(Android.Webkit.WebView? view, bool isDialog, bool isUserGesture, Message? resultMsg)
		{
			if (view?.Context is not null)
			{
				// Intercept _blank target <a> tags to always open in device browser
				// regardless of ExternalLinkMode.OpenInWebview
				var requestUrl = view.GetHitTestResult().Extra;
				var intent = new Intent(Intent.ActionView, Uri.Parse(requestUrl));
				view.Context.StartActivity(intent);
			}

			// We don't actually want to create a new WebView window so we just return false 
			return false;
		}

		public override bool OnShowFileChooser(Android.Webkit.WebView? view, IValueCallback? filePathCallback, FileChooserParams? fileChooserParams)
		{
			if (filePathCallback is null)
			{
				return base.OnShowFileChooser(view, filePathCallback, fileChooserParams);
			}

			_ = CallFilePickerAsync(filePathCallback, fileChooserParams);
			return true;
		}

		private static async Task CallFilePickerAsync(IValueCallback filePathCallback, FileChooserParams? fileChooserParams)
		{
			var pickOptions = GetPickOptions(fileChooserParams);

			if (fileChooserParams?.Mode == ChromeFileChooserMode.OpenMultiple)
			{
				var files = await FilePicker.PickMultipleAsync(pickOptions);
				if (files is null)
				{
					// Task was cancelled, return null to original callback
					filePathCallback.OnReceiveValue(null);
					return;
				}

				var androidUris = files!
					.Where(f => f is not null)
					.Select(f => new File(f.FullPath))
					.Select(Uri.FromFile)
					.Where(u => u is not null)
					.Select(u => u!)
					.ToArray();
				filePathCallback.OnReceiveValue(androidUris);
				return;
			}

			var file = await FilePicker.PickAsync(pickOptions);
			if (file is null)
			{
				// Task was cancelled, return null to original callback
				filePathCallback.OnReceiveValue(null);
				return;
			}

			var androidFile = new File(file!.FullPath);
			var androidFileUri = Uri.FromFile(androidFile);
			if (androidFileUri is null)
			{
				filePathCallback.OnReceiveValue(null);
				return;
			}

			filePathCallback.OnReceiveValue(new Uri[] { androidFileUri! });
		}

		private static PickOptions? GetPickOptions(FileChooserParams? fileChooserParams)
		{
			var acceptedFileTypes = fileChooserParams?.GetAcceptTypes();
			if (acceptedFileTypes is null ||
				// When the accept attribute isn't provided GetAcceptTypes returns: [ "" ]
				// this must be filtered out.
				(acceptedFileTypes.Length == 1 && string.IsNullOrEmpty(acceptedFileTypes[0])))
			{
				return null;
			}

			var pickOptions = new PickOptions()
			{
				FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
				{
					{ DevicePlatform.Android, acceptedFileTypes }
				})
			};
			return pickOptions;
		}
	}
}
