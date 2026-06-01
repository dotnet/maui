using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Android.Webkit;
using Microsoft.Maui;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using File = Java.IO.File;
using Uri = Android.Net.Uri;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	class BlazorWebChromeClient : WebChromeClient
	{
		private static readonly string AppOrigin = $"https://{BlazorWebView.AppHostAddress}/";
		private static readonly System.Uri AppOriginUri = new(AppOrigin);

		private readonly BlazorWebViewHandler? _handler;

		public BlazorWebChromeClient()
		{
		}

		public BlazorWebChromeClient(BlazorWebViewHandler handler)
		{
			_handler = handler;
		}

		public override bool OnCreateWindow(global::Android.Webkit.WebView? view, bool isDialog, bool isUserGesture, Message? resultMsg)
		{
			if (view?.Context is not null)
			{
				// Intercept _blank target <a> tags to always open in device browser
				// regardless of UrlLoadingStrategy.OpenInWebview
				var requestUrl = view.GetHitTestResult().Extra;

				if (!string.IsNullOrEmpty(requestUrl) && System.Uri.TryCreate(requestUrl, System.UriKind.RelativeOrAbsolute, out var uri))
				{
					var callbackArgs = UrlLoadingEventArgs.CreateWithDefaultLoadingStrategy(
						uri,
						AppOriginUri,
						Microsoft.Maui.WebNavigationTarget.NewWindow);

					if (_handler is not null)
					{
						_handler.UrlLoading(callbackArgs);
						_handler.Logger.NavigationEvent(uri, callbackArgs.UrlLoadingStrategy);
					}

					if (callbackArgs.UrlLoadingStrategy == UrlLoadingStrategy.OpenExternally)
					{
						var intent = new Intent(Intent.ActionView, Uri.Parse(requestUrl));
						view.Context.StartActivity(intent);
					}
					else if (callbackArgs.UrlLoadingStrategy == UrlLoadingStrategy.OpenInWebView)
					{
						view.LoadUrl(requestUrl!);
					}
					// CancelLoad: do nothing
				}
				else if (!string.IsNullOrEmpty(requestUrl))
				{
					// Fallback for non-parseable URIs: open externally
					var intent = new Intent(Intent.ActionView, Uri.Parse(requestUrl));
					view.Context.StartActivity(intent);
				}
			}

			// We don't actually want to create a new WebView window so we just return false 
			return false;
		}

		public override bool OnShowFileChooser(global::Android.Webkit.WebView? view, IValueCallback? filePathCallback, FileChooserParams? fileChooserParams)
		{
			if (filePathCallback is null)
			{
				return base.OnShowFileChooser(view, filePathCallback, fileChooserParams);
			}

			CallFilePickerAsync(filePathCallback, fileChooserParams).FireAndForget();
			return true;
		}

		private static async Task CallFilePickerAsync(IValueCallback filePathCallback, FileChooserParams? fileChooserParams)
		{
			var pickOptions = GetPickOptions(fileChooserParams);
			var fileResults = fileChooserParams?.Mode == ChromeFileChooserMode.OpenMultiple ?
					await FilePicker.PickMultipleAsync(pickOptions) :
					new[] { (await FilePicker.PickAsync(pickOptions))! };

			if (fileResults?.All(f => f is null) ?? true)
			{
				// Task was cancelled, return null to original callback
				filePathCallback.OnReceiveValue(null);
				return;
			}

			var fileUris = new List<Uri>(fileResults.Count());
			foreach (var fileResult in fileResults)
			{
				if (fileResult is null)
				{
					continue;
				}

				var javaFile = new File(fileResult.FullPath);
				var androidUri = Uri.FromFile(javaFile);

				if (androidUri is not null)
				{
					fileUris.Add(androidUri);
				}
			}

			filePathCallback.OnReceiveValue(fileUris.ToArray());
			return;
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
