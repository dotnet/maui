using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

		private const string NotifyCssUpdatedScript =
@"export function notifyCssUpdated(matchPathSuffix) {
	const allLinkElems = Array.from(document.querySelectorAll('link[rel=stylesheet]'));
	allLinkElems.forEach(elem => {
		const url = new URL(elem.href);
		if (url.pathname.endsWith(matchPathSuffix)) {
			url.searchParams.set('reload_version', Date.now());
			elem.href = url.toString();
		}
	});
}";

		private static readonly Dictionary<(string AssemblyName, string RelativePath), (string? ContentType, byte[] Content)> _updatedContent = new()
		{
			{ (ApplicationAssemblyName, "_framework/static-content-hot-reload.js"), ("text/javascript", Encoding.UTF8.GetBytes(NotifyCssUpdatedScript)) }
		};

		private static readonly object _updatedContentLock = new();

		private static readonly ConditionalWeakTable<WebViewManager, AttachmentState> s_attachmentStates = new();

		private const string NotifierSelector = "body::after";

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

			lock (_updatedContentLock)
			{
				_updatedContent[(assemblyName, relativePath)] = (ContentType: null, Content: contents);
			}

			OnContentUpdated?.Invoke(assemblyName, relativePath);
		}

		public static Task? TryAttachToWebViewManager(WebViewManager manager)
		{
			if (!MetadataUpdater.IsSupported)
			{
				return null;
			}

			// The value factory is deliberately side-effect free. ConditionalWeakTable may invoke it more
			// than once for concurrent callers, while AttachmentState serializes the actual registration.
			var state = s_attachmentStates.GetValue(
				manager,
				static m => new AttachmentState(
					() => m.AddRootComponentAsync(typeof(StaticContentChangeNotifier), NotifierSelector, ParameterView.Empty),
					() => m.RemoveRootComponentAsync(NotifierSelector)));
			return state.Attach();
		}

		public static Task? TryDetachFromWebViewManager(WebViewManager manager)
		{
			if (!s_attachmentStates.TryGetValue(manager, out var state))
			{
				return null;
			}

			return state.Detach();
		}

		/// <summary>
		/// Looks up hot-reloaded content for a request without taking ownership of any response state.
		/// </summary>
		public static bool TryGetUpdatedContent(string contentRootRelativePath, string requestAbsoluteUri, out byte[]? content, out string? contentType)
		{
			if (MetadataUpdater.IsSupported)
			{
				var (assemblyName, relativePath) = GetAssemblyNameAndRelativePath(requestAbsoluteUri, contentRootRelativePath);
				lock (_updatedContentLock)
				{
					if (_updatedContent.TryGetValue((assemblyName, relativePath), out var values))
					{
						content = values.Content;
						contentType = string.IsNullOrEmpty(values.ContentType) ? null : values.ContentType;
						return true;
					}
				}
			}

			content = null;
			contentType = null;
			return false;
		}

		public static bool TryReplaceResponseContent(string contentRootRelativePath, string requestAbsoluteUri, ref int responseStatusCode, ref Stream responseContent, IDictionary<string, string> responseHeaders)
		{
			if (TryGetUpdatedContent(contentRootRelativePath, requestAbsoluteUri, out var content, out var contentType))
			{
				responseStatusCode = 200;
				responseContent.Close();
				responseContent = new MemoryStream(content!);
				if (contentType is not null)
				{
					responseHeaders["Content-Type"] = contentType;
				}

				return true;
			}

			return false;
		}

		internal sealed class AttachmentState
		{
			private readonly object _lock = new();
			private readonly Func<Task> _addNotifier;
			private readonly Func<Task> _removeNotifier;
			private Task _lastOperation = Task.CompletedTask;
			private Task? _attachTask;

			public AttachmentState(Func<Task> addNotifier, Func<Task> removeNotifier)
			{
				_addNotifier = addNotifier;
				_removeNotifier = removeNotifier;
			}

			public Task Attach()
			{
				lock (_lock)
				{
					if (_attachTask is not null)
					{
						return _attachTask;
					}

					var operation = _lastOperation.IsCompletedSuccessfully
						? _addNotifier()
						: AddNotifierAfterAsync(_lastOperation);
					var attachTask = operation.IsCompletedSuccessfully
						? operation
						: CompleteAttachAsync(operation);

					_attachTask = attachTask;
					_lastOperation = attachTask;

					if (!attachTask.IsCompletedSuccessfully)
					{
						_ = attachTask.ContinueWith(
							static (completedTask, state) => ((AttachmentState)state!).OnAttachCompleted(completedTask),
							this,
							CancellationToken.None,
							TaskContinuationOptions.ExecuteSynchronously,
							TaskScheduler.Default);
					}

					return attachTask;
				}
			}

			public Task? Detach()
			{
				lock (_lock)
				{
					if (_attachTask is null)
					{
						return null;
					}

					var detachTask = RemoveNotifierAsync(_attachTask);
					_attachTask = null;
					_lastOperation = detachTask;
					return detachTask;
				}
			}

			private async Task AddNotifierAfterAsync(Task precedingOperation)
			{
				await precedingOperation.ConfigureAwait(false);
				await _addNotifier().ConfigureAwait(false);
			}

			private async Task CompleteAttachAsync(Task attachOperation)
			{
				try
				{
					await attachOperation.ConfigureAwait(false);
				}
				catch (Exception attachException)
				{
					try
					{
						// WebViewManager reserves the selector before its asynchronous render work.
						// Remove that reservation before allowing a later attach to retry.
						await RemoveNotifierAsync(Task.CompletedTask).ConfigureAwait(false);
					}
					catch (Exception cleanupException)
					{
						throw new AggregateException(
							"The static content hot reload notifier failed to attach and could not be removed.",
							attachException,
							cleanupException);
					}

					throw;
				}
			}

			private async Task RemoveNotifierAsync(Task attachTask)
			{
				try
				{
					await attachTask.ConfigureAwait(false);
					await _removeNotifier().ConfigureAwait(false);
				}
				catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
				{
					// The manager was disposed, or its renderer torn down, while this detach was in flight.
					// Detaching only promises that the notifier is no longer registered, and a disposed manager
					// satisfies that, so this completes successfully. Without this the caller would have to
					// await the task purely to avoid an unobservable fault during teardown.
				}
			}

			private void OnAttachCompleted(Task attachTask)
			{
				if (attachTask.IsCompletedSuccessfully)
				{
					return;
				}

				// Observe discarded failures and clear them so a later attach can retry.
				_ = attachTask.Exception;

				lock (_lock)
				{
					if (ReferenceEquals(_attachTask, attachTask))
					{
						_attachTask = null;
					}

					if (ReferenceEquals(_lastOperation, attachTask))
					{
						_lastOperation = Task.CompletedTask;
					}
				}
			}
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
						// CSS hot reload being broken by customizations, we'll have the JS-side refresh stylesheets with a matching filename.
						// Most of the time this will only reload a single stylesheet.

						string matchPathSuffix = "/" + Path.GetFileName(relativePath);
						if (matchPathSuffix.EndsWith(".bundle.scp.css"))
						{
							// Bundles from class libraries are imported in the <Project>.styles.css file,
							// so match that file instead of the bundle.
							matchPathSuffix = ".styles.css";
						}

						await module.InvokeVoidAsync("notifyCssUpdated", matchPathSuffix);
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
