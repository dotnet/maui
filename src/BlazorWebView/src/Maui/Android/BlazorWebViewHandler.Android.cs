using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Android.Webkit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Microsoft.Maui.Handlers;
using static Android.Views.ViewGroup;
using AWebView = Android.Webkit.WebView;
using Path = System.IO.Path;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	public partial class BlazorWebViewHandler : ViewHandler<IBlazorWebView, AWebView>
	{
		private WebViewClient? _webViewClient;
		private WebChromeClient? _webChromeClient;
		private AndroidWebKitWebViewManager? _webviewManager;
		internal AndroidWebKitWebViewManager? WebviewManager => _webviewManager;

		protected override AWebView CreateNativeView()
		{
			var aWebView = new AWebView(Context!)
			{
#pragma warning disable 618 // This can probably be replaced with LinearLayout(LayoutParams.MatchParent, LayoutParams.MatchParent); just need to test that theory
				LayoutParameters = new Android.Widget.AbsoluteLayout.LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent, 0, 0)
#pragma warning restore 618
			};

			AWebView.SetWebContentsDebuggingEnabled(enabled: true);

			if (aWebView.Settings != null)
			{
				aWebView.Settings.JavaScriptEnabled = true;
				aWebView.Settings.DomStorageEnabled = true;
			}

			_webViewClient = GetWebViewClient();
			aWebView.SetWebViewClient(_webViewClient);

			_webChromeClient = GetWebChromeClient();
			aWebView.SetWebChromeClient(_webChromeClient);

			return aWebView;
		}

		protected override void DisconnectHandler(AWebView nativeView)
		{
			nativeView.StopLoading();

			_webViewClient?.Dispose();
			_webChromeClient?.Dispose();
		}

		private bool RequiredStartupPropertiesSet =>
			//_webview != null &&
			HostPage != null &&
			Services != null;

		private void StartWebViewCoreIfPossible()
		{
			if (!RequiredStartupPropertiesSet ||
				_webviewManager != null)
			{
				return;
			}
			if (NativeView == null)
			{
				throw new InvalidOperationException($"Can't start {nameof(BlazorWebView)} without native web view instance.");
			}

			var assetConfig = Services!.GetRequiredService<BlazorAssetsAssemblyConfiguration>()!;

			// We assume the host page is always in the root of the content directory, because it's
			// unclear there's any other use case. We can add more options later if so.
			var contentRootDir = Path.GetDirectoryName(HostPage!) ?? string.Empty;
			var hostPageRelativePath = Path.GetRelativePath(contentRootDir, HostPage!);

			var fileProvider = new MauiAssetFileProvider(contentRootDir);

			_webviewManager = new AndroidWebKitWebViewManager(this, NativeView, Services!, MauiDispatcher.Instance, fileProvider, hostPageRelativePath);
			if (RootComponents != null)
			{
				foreach (var rootComponent in RootComponents)
				{
					// Since the page isn't loaded yet, this will always complete synchronously
					_ = rootComponent.AddToWebViewManagerAsync(_webviewManager);
				}
			}

			_webviewManager.Navigate("/");
		}

		protected virtual WebViewClient GetWebViewClient() =>
			new WebKitWebViewClient(this);

		protected virtual WebChromeClient GetWebChromeClient() =>
			new WebChromeClient();

		public partial class MauiAssetFileProvider : IFileProvider
		{
			private string _contentRootDir;

			public MauiAssetFileProvider(string contentRootDir)
			{
				_contentRootDir = contentRootDir;
			}

			public IDirectoryContents GetDirectoryContents(string subpath)
				=> new AndroidMauiAssetDirectoryContents(Path.Combine(_contentRootDir, subpath));

			public IFileInfo GetFileInfo(string subpath)
				=> new AndroidMauiAssetFileInfo(Path.Combine(_contentRootDir, subpath));

			public IChangeToken? Watch(string filter)
				=> null;
		}

		class AndroidMauiAssetFileInfo : IFileInfo
		{
			public AndroidMauiAssetFileInfo(string asset)
			{
				var itemsCount = Android.App.Application.Context.Assets?.List(asset)?.Length ?? 0;

				PhysicalPath = asset;
				IsDirectory = itemsCount > 0;
				Length = IsDirectory ? itemsCount : 1;
				Name = IsDirectory
					? new DirectoryInfo(asset)?.Name ?? asset
					: Path.GetFileName(asset);
			}

			public bool Exists => true;
			public long Length { get; }
			public string PhysicalPath { get; }
			public string Name { get; }
			public DateTimeOffset LastModified { get; }
			public bool IsDirectory { get; }

			public Stream CreateReadStream()
				=> Android.App.Application.Context.Assets?.Open(PhysicalPath)
					?? throw new FileNotFoundException();
		}

		class AndroidMauiAssetDirectoryContents : IDirectoryContents
		{
			public AndroidMauiAssetDirectoryContents(string subpath)
			{
				var sep = Java.IO.File.Separator ?? "/";

				var dir = subpath.Replace("/", sep);

				var assets = Android.App.Application.Context.Assets?.List(dir);

				foreach (var a in assets ?? Array.Empty<string>())
					files.Add(new AndroidMauiAssetFileInfo(subpath.TrimEnd(sep.ToCharArray()) + sep + a));
			}

			List<AndroidMauiAssetFileInfo> files = new List<AndroidMauiAssetFileInfo>();

			public bool Exists
				=> files.Any();

			public IEnumerator<IFileInfo> GetEnumerator()
				=> files.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator()
				=> files.GetEnumerator();
		}
	}
}
