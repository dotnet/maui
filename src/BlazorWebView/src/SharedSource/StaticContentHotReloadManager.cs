using System;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.JSInterop;

[assembly: MetadataUpdateHandler(typeof(Microsoft.AspNetCore.Components.WebView.StaticContentHotReloadManager))]

namespace Microsoft.AspNetCore.Components.WebView
{
	internal sealed class StaticContentHotReloadManager
	{
		public static readonly StaticContentHotReloadManager Default = new();

		private delegate void ContentUpdateHandler(string assemblyName, string relativePath, byte[] contents);
		private event ContentUpdateHandler? OnContentUpdate;

		/// <summary>
		/// MetadataUpdateHandler event. This is invoked by the hot reload host via reflection.
		/// </summary>
		public static void UpdateContent(string assemblyName, string relativePath, byte[] contents)
			=> Default.OnContentUpdate?.Invoke(assemblyName, relativePath, contents);

		public void AttachToWebViewManagerIfEnabled(WebViewManager manager)
		{
			if (MetadataUpdater.IsSupported)
			{
				manager.AddRootComponentAsync(typeof(StaticContentUpdater), "body::after", ParameterView.Empty);
			}
		}

		// To provide a consistent way of transporting the data across all platforms,
		// we can use the existing IJSRuntime. In turn we can get an instance of this
		// that's always attached to the currently-loaded page (if it's a Blazor page)
		// by injecting this headless root component.
		private class StaticContentUpdater : IComponent, IDisposable
		{
			[Inject] private IJSRuntime JSRuntime { get; set; } = default!;

			public void Attach(RenderHandle renderHandle)
			{
				Default.OnContentUpdate += HandleContentUpdate;
			}

			public void Dispose()
			{
				Default.OnContentUpdate -= HandleContentUpdate;
			}

			private void HandleContentUpdate(string assemblyName, string relativePath, byte[] contents)
			{
				throw new NotImplementedException();
			}

			public Task SetParametersAsync(ParameterView parameters)
				=> Task.CompletedTask;
		}
	}
}
