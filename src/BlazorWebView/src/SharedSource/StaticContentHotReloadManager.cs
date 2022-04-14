using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

[assembly: MetadataUpdateHandler(typeof(Microsoft.AspNetCore.Components.WebView.StaticContentHotReloadManager))]

namespace Microsoft.AspNetCore.Components.WebView
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable RS0016 // Add public types and members to the declared API
	public static class TemporaryStaticContent
	{
		public static void UpdateContent(string assemblyName, string relativePath, byte[] contents)
			=> StaticContentHotReloadManager.UpdateContent(assemblyName, relativePath, contents);
	}
#pragma warning restore RS0016 // Add public types and members to the declared API
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

	internal static class StaticContentHotReloadManager
	{
		private delegate void ContentUpdatedHandler(string assemblyName, string relativePath);
		private static event ContentUpdatedHandler? OnContentUpdated;

		private static string ApplicationAssemblyName { get; } =
#if MAUI
			Application.Context.PackageName;
#else
			Assembly.GetEntryAssembly()!.GetName().Name!;
#endif

		private static readonly Dictionary<(string AssemblyName, string RelativePath), (string? ContentType, byte[] Content)> _updatedContent = new()
		{
			{ (ApplicationAssemblyName, "_framework/static-content-hot-reload.js"), ("text/javascript", Encoding.UTF8.GetBytes(@"
	export function notifyContentUpdated(urlWithinOrigin) {
		const allLinkElems = Array.from(document.querySelectorAll('link[rel=stylesheet]'));
		const absoluteUrl = document.location.origin + urlWithinOrigin;
		const matchingLinkElems = allLinkElems.filter(x => x.href === absoluteUrl);

		// If we can't find a matching link element, that probably means it's a CSS file imported via @import
		// from some other CSS file. We can't know which other file imports it, so refresh them all.
		const linkElemsToUpdate = matchingLinkElems.length > 0 || !absoluteUrl.endsWith('.css')
			? matchingLinkElems
			: allLinkElems;

		linkElemsToUpdate.forEach(tag => tag.href += '');
	}
")) }
		};

		/// <summary>
		/// MetadataUpdateHandler event. This is invoked by the hot reload host via reflection.
		/// </summary>
		public static void UpdateContent(string assemblyName, string relativePath, byte[] content)
		{
			_updatedContent[(assemblyName, relativePath)] = (ContentType: null, Content: content);
			OnContentUpdated?.Invoke(assemblyName, relativePath);
		}

		public static void AttachToWebViewManagerIfEnabled(WebViewManager manager, string assemblyName, string contentRoot)
		{
			if (MetadataUpdater.IsSupported)
			{
				var parameters = new Dictionary<string, object?> { { nameof(StaticContentUpdater.ContentRoot), contentRoot } };
				manager.AddRootComponentAsync(typeof(StaticContentUpdater), "body::after", ParameterView.FromDictionary(parameters));
			}
		}

		public static bool TryReplaceResponseContent(string contentRootRelativePath, string requestAbsoluteUri, ref int responseStatusCode, ref Stream responseContent, IDictionary<string, string> responseHeaders)
		{
			if (MetadataUpdater.IsSupported)
			{
				var (assemblyName, relativePath) = GetAssemblyNameAndRelativePath(requestAbsoluteUri, contentRootRelativePath);
				if (_updatedContent.TryGetValue((assemblyName, relativePath), out var values))
				{
					responseStatusCode = 200;
					responseContent = new MemoryStream(values.Content);
					if (!string.IsNullOrEmpty(values.ContentType))
					{
						responseHeaders["Content-Type"] = values.ContentType;
					}

					return true;
				}
			}

			return false;
		}

		private readonly static Regex ContentUrlRegex = new Regex("^_content/(?<AssemblyName>[^/]+)/(?<RelativePath>.*)");
		
		private static (string AssemblyName, string RelativePath) GetAssemblyNameAndRelativePath(string requestAbsoluteUri, string appContentRoot)
		{
			var requestPath = new Uri(requestAbsoluteUri).AbsolutePath.Substring(1);
			if (ContentUrlRegex.Match(requestPath) is { Success: true } match)
			{
				// For RCLs (i.e., URLs of the form _content/assembly/path), we assume the content root within the
				// RCL to be "wwwroot" since we have no other information. If this is not the case, content within
				// that RCL will not be hot-reloadable.
				return (match.Groups["AssemblyName"].Value, $"wwwroot/{match.Groups["RelativePath"].Value}");
			}
			else if (requestPath.StartsWith("_framework/", StringComparison.Ordinal))
			{
				return (ApplicationAssemblyName, requestPath);
			}
			else
			{
				return (ApplicationAssemblyName, Path.Combine(appContentRoot, requestPath).Replace('\\', '/'));
			}
		}

		// To provide a consistent way of transporting the data across all platforms,
		// we can use the existing IJSRuntime. In turn we can get an instance of this
		// that's always attached to the currently-loaded page (if it's a Blazor page)
		// by injecting this headless root component.
		private sealed class StaticContentUpdater : IComponent, IDisposable
		{
			private ILogger _logger = default!;

			[Inject] private IJSRuntime JSRuntime { get; set; } = default!;
			[Inject] private ILoggerFactory LoggerFactory { get; set; } = default!;
			[Parameter] public string ContentRoot { get; set; } = default!;

			public void Attach(RenderHandle renderHandle)
			{
				_logger = LoggerFactory.CreateLogger<StaticContentUpdater>();
				OnContentUpdated += NotifyContentUpdated;
			}

			public void Dispose()
			{
				OnContentUpdated -= NotifyContentUpdated;
			}

			private void NotifyContentUpdated(string assemblyName, string relativePath)
			{
				// It handles its own errors
				_ = NotifyContentUpdatedAsync(assemblyName, relativePath);
			}

			private async Task NotifyContentUpdatedAsync(string assemblyName, string relativePath)
			{
				try
				{
					await using var module = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_framework/static-content-hot-reload.js");

					if (string.Equals(assemblyName, ApplicationAssemblyName, StringComparison.Ordinal))
					{
						if (relativePath.StartsWith(ContentRoot + "/", StringComparison.Ordinal))
						{
							var pathWithinContentRoot = relativePath.Substring(ContentRoot.Length);
							await module.InvokeVoidAsync("notifyContentUpdated", pathWithinContentRoot);
						}
					}
					else
					{
						if (relativePath.StartsWith("wwwroot/", StringComparison.Ordinal))
						{
							var pathWithinContentRoot = relativePath.Substring("wwwroot/".Length);
							await module.InvokeVoidAsync("notifyContentUpdated", $"/_content/{assemblyName}/{pathWithinContentRoot}");
						}
					}
					
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to notify about static content update to {relativePath}.");
				}
			}

			public Task SetParametersAsync(ParameterView parameters)
			{
				parameters.SetParameterProperties(this);
				return Task.CompletedTask;
			}
		}
	}
}
