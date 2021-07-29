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

			var compositeFileProvider = new MauiAssetFileProvider(contentRootDir);

			_webviewManager = new AndroidWebKitWebViewManager(this, NativeView, Services!, MauiDispatcher.Instance, compositeFileProvider, hostPageRelativePath);
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
			{
				var path = Path.Combine(_contentRootDir, subpath);
				try
				{
					var file = Android.App.Application.Context.Assets!.Open(path);
					Func<Stream> stream = () => Android.App.Application.Context.Assets!.Open(path);
					return new AndroidMauiAssetFileInfo(Path.GetFileName(path), stream);
				}
				catch (Exception)
				{
					return new NotFoundFileInfo(Path.GetFileName(subpath));
				}
			}

			public IChangeToken? Watch(string filter)
				=> null;
		}

		class AndroidMauiAssetFileInfo : IFileInfo
		{
			private Func<Stream> _factory;

			public AndroidMauiAssetFileInfo(string name, Func<Stream> factory)
			{
				Name = name;
				_factory = factory;
				using var stream = factory();
				using var memoryStream = new MemoryStream();
				stream.CopyTo(memoryStream);
				memoryStream.Seek(0, SeekOrigin.Begin);
				Length = memoryStream.Length;
			}

			public bool Exists => true;
			public long Length { get; }
			public string PhysicalPath { get; } = null!;
			public string Name { get; }
			public DateTimeOffset LastModified { get; } = DateTimeOffset.FromUnixTimeSeconds(0);
			public bool IsDirectory { get; } = false;

			public Stream CreateReadStream()
				=> _factory();
		}

		class AndroidMauiAssetDirectoryContents : IDirectoryContents
		{
			public AndroidMauiAssetDirectoryContents(string subpath)
			{
			}

			List<AndroidMauiAssetFileInfo> files = new List<AndroidMauiAssetFileInfo>();

			public bool Exists => false;

			public IEnumerator<IFileInfo> GetEnumerator()
				=> throw new NotImplementedException();

			IEnumerator IEnumerable.GetEnumerator()
				=> throw new NotImplementedException();
		}
	}
}
