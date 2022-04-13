using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

[assembly: MetadataUpdateHandler(typeof(Microsoft.AspNetCore.Components.WebView.StaticContentHotReloadManager))]

namespace Microsoft.AspNetCore.Components.WebView
{
	internal static class StaticContentHotReloadManager
	{
		private delegate void ContentUpdatedHandler(string url);
		private static event ContentUpdatedHandler? OnContentUpdated;
		private static string AppOrigin = default!;

		private static readonly Dictionary<string, (string? ContentType, byte[] Content)> _updatedContentByAbsoluteUrl = new(StringComparer.Ordinal);

		private static readonly string StaticContentHotReloadModuleSource = @"
	export function notifyContentUpdated(url) {
		const tagsToUpdate = Array.from(document.querySelectorAll('link[rel=stylesheet]')).filter(x => x.href === url);
		tagsToUpdate.forEach(tag => tag.href += '');
	}
";

		/// <summary>
		/// MetadataUpdateHandler event. This is invoked by the hot reload host via reflection.
		/// </summary>
		public static void UpdateContent(string assemblyName, string relativePath, byte[] content)
		{
			var absoluteUrl = GetAbsoluteUrlForStaticContent(assemblyName, relativePath);
			_updatedContentByAbsoluteUrl[absoluteUrl] = (ContentType: null, Content: content);
			OnContentUpdated?.Invoke(absoluteUrl);
		}

		public static void AttachToWebViewManagerIfEnabled(WebViewManager manager, string appOrigin)
		{
			if (MetadataUpdater.IsSupported)
			{
				AppOrigin = appOrigin;
				_updatedContentByAbsoluteUrl[AppOrigin + "_framework/static-content-hot-reload.js"] =
					(ContentType: "text/javascript", Content: Encoding.UTF8.GetBytes(StaticContentHotReloadModuleSource));

				manager.AddRootComponentAsync(typeof(StaticContentUpdater), "body::after", ParameterView.Empty);
			}
		}

		public static bool TryReplaceResponseContent(string requestAbsoluteUri, ref int responseStatusCode, ref Stream responseContent, IDictionary<string, string> responseHeaders)
		{
			if (MetadataUpdater.IsSupported && _updatedContentByAbsoluteUrl.TryGetValue(requestAbsoluteUri, out var values))
			{
				responseStatusCode = 200;
				responseContent = new MemoryStream(values.Content);
				if (!string.IsNullOrEmpty(values.ContentType))
				{
					responseHeaders["Content-Type"] = values.ContentType;
				}

				return true;
			}
			else
			{
				return false;
			}
		}
		
		private static string GetAbsoluteUrlForStaticContent(string assemblyName, string relativePath)
		{
			// This logic might not cover every circumstance if the developer customizes the host page path
			// or is doing something custom with static web assets. However it should cover any mainstream
			// case with single projects and RCLs. We may have to allow for other cases in the future, or
			// may have to receive different information from tooling.

			// Since scoped CSS bundles might not have a wwwroot prefix, normalize by removing it.
			// Whether this is really needed depends on tooling implementations that are not yet known.
			const string wwwrootPrefix = "wwwroot/";
			if (relativePath.StartsWith(wwwrootPrefix, StringComparison.Ordinal))
			{
				relativePath = relativePath.Substring(wwwrootPrefix.Length);
			}

			if (relativePath.StartsWith("/"))
			{
				relativePath = relativePath.Substring(1);
			}

			// SWA convention for RCLs
			if (!string.Equals(assemblyName, Assembly.GetEntryAssembly()!.GetName().Name, StringComparison.Ordinal))
			{
				relativePath = $"_content/{assemblyName}/{relativePath}";
			}

			return AppOrigin + relativePath;
		}

		// To provide a consistent way of transporting the data across all platforms,
		// we can use the existing IJSRuntime. In turn we can get an instance of this
		// that's always attached to the currently-loaded page (if it's a Blazor page)
		// by injecting this headless root component.
		private sealed class StaticContentUpdater : IComponent, IDisposable
		{
			[Inject] private IJSRuntime JSRuntime { get; set; } = default!;
			[Inject] private ILoggerFactory LoggerFactory { get; set; } = default!;
			private ILogger _logger = default!;

			public void Attach(RenderHandle renderHandle)
			{
				_logger = LoggerFactory.CreateLogger<StaticContentUpdater>();
				OnContentUpdated += NotifyContentUpdated;
			}

			public void Dispose()
			{
				OnContentUpdated -= NotifyContentUpdated;
			}

			private void NotifyContentUpdated(string url)
			{
				// It handles its own errors
				_ = NotifyContentUpdatedAsync(url);
			}

			private async Task NotifyContentUpdatedAsync(string url)
			{
				try
				{
					await using var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_framework/static-content-hot-reload.js");
					await module.InvokeVoidAsync("notifyContentUpdated", url);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to notify about static content update to {url}.");
				}
			}

			public Task SetParametersAsync(ParameterView parameters)
				=> Task.CompletedTask;
		}
	}
}
