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
	internal static class StaticContentHotReloadManager
	{
		private delegate void ContentUpdatedHandler(string assemblyName, string relativePath);

		private readonly static Regex ContentUrlRegex = new Regex("^_content/(?<AssemblyName>[^/]+)/(?<RelativePath>.*)");
		private static event ContentUpdatedHandler? OnContentUpdated;

		// If the current platform can't tell us the application entry assembly name, we can use a placeholder name
		private static string ApplicationAssemblyName { get; } = Assembly.GetEntryAssembly()?.GetName().Name
			?? "__application_assembly__";

		private static readonly Dictionary<(string AssemblyName, string RelativePath), (string? ContentType, byte[] Content)> _updatedContent = new()
		{
			{ (ApplicationAssemblyName, "_framework/static-content-hot-reload.js"), ("text/javascript", Encoding.UTF8.GetBytes(@"
	export function notifyCssUpdated() {
		const allLinkElems = Array.from(document.querySelectorAll('link[rel=stylesheet]'));
		allLinkElems.forEach(elem => elem.href += '');
	}
")) }
		};

		/// <summary>
		/// MetadataUpdateHandler event. This is invoked by the hot reload host via reflection.
		/// </summary>
		public static void UpdateContent(string assemblyName, bool isApplicationProject, string relativePath, byte[] contents)
		{
			if (isApplicationProject)
			{
				// Some platforms don't know the name of the application entry assembly (e.g., Android) so in
				// those cases we have a placeholder name for it. The tooling does know the real name, but we
				// need to use our placeholder so the lookups work later.
				assemblyName = ApplicationAssemblyName;
			}

			_updatedContent[(assemblyName, relativePath)] = (ContentType: null, Content: contents);
			OnContentUpdated?.Invoke(assemblyName, relativePath);
		}

		public static void AttachToWebViewManagerIfEnabled(WebViewManager manager)
		{
			if (MetadataUpdater.IsSupported)
			{
				manager.AddRootComponentAsync(typeof(StaticContentChangeNotifier), "body::after", ParameterView.Empty);
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
					responseContent.Close();
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

		private static (string AssemblyName, string RelativePath) GetAssemblyNameAndRelativePath(string requestAbsoluteUri, string appContentRoot)
		{
			var requestPath = new Uri(requestAbsoluteUri).AbsolutePath.Substring(1);
			if (ContentUrlRegex.Match(requestPath) is { Success: true } match)
			{
				var assemblyName = match.Groups["AssemblyName"].Value;
				var relativePath = match.Groups["RelativePath"].Value;

				// Remove the fingerprint from scoped CSS bundles, since CSS hot reload will send new content without the fingerprint.
				// The relative path for *.bundle.scp.css is just the file name, since they are always directly in the assembly's content directory.
				// Example: LibraryName.<fingerprint>.bundle.scp.css -> LibraryName.bundle.scp.css
				if (relativePath.StartsWith($"{assemblyName}.", StringComparison.Ordinal) && relativePath.EndsWith(".bundle.scp.css", StringComparison.Ordinal))
				{
					relativePath = $"{assemblyName}.bundle.scp.css";
				}

				// For RCLs (i.e., URLs of the form _content/assembly/path), we assume the content root within the
				// RCL to be "wwwroot" since we have no other information. If this is not the case, content within
				// that RCL will not be hot-reloadable.
				return (assemblyName, $"wwwroot/{relativePath}");
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
		private sealed class StaticContentChangeNotifier : IComponent, IDisposable
		{
			private ILogger _logger = default!;

			[Inject] private IJSRuntime JSRuntime { get; set; } = default!;
			[Inject] private ILoggerFactory LoggerFactory { get; set; } = default!;

			public void Attach(RenderHandle renderHandle)
			{
				_logger = LoggerFactory.CreateLogger<StaticContentChangeNotifier>();
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

					// In the future we might want to hot-reload other content types such as images, but currently the tooling is
					// only expected to notify about CSS files. If it notifies us about something else, we'd need different JS logic.
					if (string.Equals(".css", Path.GetExtension(relativePath), StringComparison.Ordinal))
					{
						// We could try to supply the URL of the modified file, so the JS-side logic could only update the affected
						// stylesheet. This would reduce flicker. However, this involves hardcoding further details about URL conventions
						// (e.g., _content/AssemblyName/Path) and accounting for configurable content roots. To reduce the chances of
						// CSS hot reload being broken by customizations, we'll have the JS-side code refresh all stylesheets.
						await module.InvokeVoidAsync("notifyCssUpdated");
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, $"Failed to notify about static content update to {relativePath}.");
				}
			}

			public Task SetParametersAsync(ParameterView parameters)
				=> Task.CompletedTask;
		}
	}
}
