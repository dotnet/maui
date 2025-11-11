#if __IOS__ || MACCATALYST
using PlatformView = WebKit.WKWebView;
using HeaderPairType = Foundation.NSObject;
#elif MONOANDROID
using PlatformView = Android.Webkit.WebView;
using HeaderPairType = System.String;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.WebView2;
using HeaderPairType = System.String;
#elif TIZEN
using PlatformView = Tizen.NUI.BaseComponents.View;
using HeaderPairType = System.String;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !ANDROID && !TIZEN)
using PlatformView = System.Object;
using HeaderPairType = System.String;
#endif

#if __ANDROID__
using Android.Webkit;
#elif __IOS__
using WebKit;
#endif
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Handlers
{
	[RequiresUnreferencedCode(DynamicFeatures)]
#if !NETSTANDARD
	[RequiresDynamicCode(DynamicFeatures)]
#endif
	public partial class HybridWebViewHandler : IHybridWebViewHandler
	{
		internal const string DynamicFeatures = "HybridWebView uses dynamic System.Text.Json serialization features.";
		internal const string NotSupportedMessage = DynamicFeatures + " Enable the $(MauiHybridWebViewSupported) property in your .csproj file to use in a trimming unsafe manner.";

		// Using an IP address means that the web view doesn't wait for any DNS resolution,
		// making it substantially faster. Note that this isn't real HTTP traffic, since
		// we intercept all the requests within this origin.
		private static readonly string AppHostAddress = "0.0.0.1";

		private static readonly string AppHostScheme =
#if IOS || MACCATALYST
			"app";
#else
			"https";
#endif

		/// <summary>
		/// Gets the application's base URI. Defaults to <c>https://0.0.0.1/</c> on Windows and Android,
		/// and <c>app://0.0.0.1/</c> on iOS and MacCatalyst (because <c>https</c> is reserved).
		/// </summary>
		internal static readonly string AppOrigin = $"{AppHostScheme}://{AppHostAddress}/";

		internal static readonly Uri AppOriginUri = new(AppOrigin);

		internal const string InvokeDotNetPath = "__hwvInvokeDotNet";
		internal const string HybridWebViewDotJsPath = "_framework/hybridwebview.js";

		internal const string InvokeDotNetTokenHeaderName = "X-Maui-Invoke-Token";
		internal const string InvokeDotNetTokenHeaderValue = "HybridWebView";
		internal const string InvokeDotNetBodyHeaderName = "X-Maui-Request-Body";


		public static IPropertyMapper<IHybridWebView, IHybridWebViewHandler> Mapper = new PropertyMapper<IHybridWebView, IHybridWebViewHandler>(ViewHandler.ViewMapper)
		{
#if WINDOWS
			[nameof(IView.FlowDirection)] = MapFlowDirection,
#endif
		};

		public static CommandMapper<IHybridWebView, IHybridWebViewHandler> CommandMapper = new(ViewCommandMapper)
		{
			[nameof(IHybridWebView.EvaluateJavaScriptAsync)] = MapEvaluateJavaScriptAsync,
			[nameof(IHybridWebView.InvokeJavaScriptAsync)] = MapInvokeJavaScriptAsync,
			[nameof(IHybridWebView.SendRawMessage)] = MapSendRawMessage,
		};

		public HybridWebViewHandler() : base(Mapper, CommandMapper)
		{
		}

		public HybridWebViewHandler(IPropertyMapper? mapper = null, CommandMapper? commandMapper = null)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper)
		{
		}

		IHybridWebView IHybridWebViewHandler.VirtualView => VirtualView;

		PlatformView IHybridWebViewHandler.PlatformView => PlatformView;

		internal HybridWebViewDeveloperTools DeveloperTools => MauiContext?.Services.GetService<HybridWebViewDeveloperTools>() ?? new HybridWebViewDeveloperTools();

		private const string InvokeJavaScriptThrowsExceptionsSwitch = "HybridWebView.InvokeJavaScriptThrowsExceptions";

		private static bool IsInvokeJavaScriptThrowsExceptionsEnabled =>
			!AppContext.TryGetSwitch(InvokeJavaScriptThrowsExceptionsSwitch, out var enabled) || enabled;

		void MessageReceived(string rawMessage) =>
			HybridWebViewHelper.ProcessRawMessage(this, VirtualView, rawMessage);

		internal async Task<byte[]?> InvokeDotNetAsync(Stream? streamBody = null, string? stringBody = null)
		{
			var logger = MauiContext?.CreateLogger<HybridWebViewHandler>();
			return await HybridWebViewHelper.ProcessInvokeDotNetAsync(
				VirtualView?.InvokeJavaScriptTarget,
				VirtualView?.InvokeJavaScriptType,
				logger,
				streamBody,
				stringBody);
		}

#if PLATFORM && !TIZEN
		public static async void MapEvaluateJavaScriptAsync(IHybridWebViewHandler handler, IHybridWebView hybridWebView, object? arg)
		{
			if (arg is not EvaluateJavaScriptAsyncRequest request)
			{
				return;
			}

			if (handler.PlatformView is null)
			{
				request.SetCanceled();
				return;
			}

			try
			{
				// Delegate to helper for all processing logic
				var result = await HybridWebViewHelper.ProcessEvaluateJavaScriptAsync(handler, hybridWebView, request);

				request.SetResult(result!);
			}
			catch (Exception ex)
			{
				request.SetException(ex);
			}
		}

		public static async void MapInvokeJavaScriptAsync(IHybridWebViewHandler handler, IHybridWebView hybridWebView, object? arg)
		{
			if (arg is not HybridWebViewInvokeJavaScriptRequest request)
			{
				return;
			}

			if (handler.PlatformView is null)
			{
				request.SetCanceled();
				return;
			}

			try
			{
				// Delegate to helper for all processing logic
				var result = await HybridWebViewHelper.ProcessInvokeJavaScriptAsync(handler, hybridWebView, request);

				request.SetResult(result);
			}
			catch (Exception ex)
			{
				request.SetException(ex);
			}
		}
#endif

		internal static async Task<Stream?> GetAssetStreamAsync(string assetPath)
		{
			if (!await FileSystem.AppPackageFileExistsAsync(assetPath))
			{
				return null;
			}
			return await FileSystem.OpenAppPackageFileAsync(assetPath);
		}

		internal static Stream? GetEmbeddedStream(string embeddedPath)
		{
			var assembly = typeof(HybridWebViewHandler).Assembly;

			var resourceName = assembly
				.GetManifestResourceNames()
				.FirstOrDefault(name => name.Equals(embeddedPath.Replace('\\', '/'), StringComparison.OrdinalIgnoreCase));

			if (resourceName is null)
			{
				return null;
			}

			return assembly.GetManifestResourceStream(resourceName);
		}

		private static bool IsExpectedHeader((string? Name, string? Value) header, (string Name, string Value) expected) =>
			string.Equals(header.Name, expected.Name, StringComparison.OrdinalIgnoreCase) &&
			string.Equals(header.Value, expected.Value, StringComparison.OrdinalIgnoreCase);

		internal static bool HasExpectedHeaders(IEnumerable<KeyValuePair<HeaderPairType, HeaderPairType>>? headers)
		{
			if (headers is null)
				return false;

			var expectedOrigin = AppOrigin.TrimEnd('/');

			var hasExpectedToken = false;
			var hasExpectedOrigin = false;
			var hasExpectedReferer = false;

			foreach (var header in headers)
			{
#if IOS || MACCATALYST
				var name = header.Key?.ToString();
				var value = header.Value?.ToString();
#else
				var name = header.Key;
				var value = header.Value;
#endif

				// Is this from the script
				hasExpectedToken = hasExpectedToken || IsExpectedHeader((name, value), (InvokeDotNetTokenHeaderName, InvokeDotNetTokenHeaderValue));

				// Is this from a local script
				var urlValue = value?.TrimEnd('/');
				hasExpectedOrigin = hasExpectedOrigin || IsExpectedHeader((name, urlValue), ("Origin", expectedOrigin));
				hasExpectedReferer = hasExpectedReferer || IsExpectedHeader((name, urlValue), ("Referer", expectedOrigin));

				if (hasExpectedToken && (hasExpectedOrigin || hasExpectedReferer))
					return true;
			}

			return false;
		}

#if !NETSTANDARD
		internal static readonly FileExtensionContentTypeProvider ContentTypeProvider = new();
#endif
	}
}
