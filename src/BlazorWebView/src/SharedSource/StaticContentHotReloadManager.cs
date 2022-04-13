using System;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Components.Rendering;

[assembly: MetadataUpdateHandler(typeof(Microsoft.AspNetCore.Components.WebView.StaticContentHotReloadManager))]

namespace Microsoft.AspNetCore.Components.WebView
{
	internal sealed class StaticContentHotReloadManager
	{
		public static readonly StaticContentHotReloadManager Default = new();

		public bool MetadataUpdateSupported => MetadataUpdater.IsSupported;

		public event Action<(string AssemblyName, string RelativePath, byte[] Contents)>? OnContentUpdate;

		/// <summary>
		/// MetadataUpdateHandler event. This is invoked by the hot reload host via reflection.
		/// </summary>
		public static void UpdateContent(string assemblyName, string relativePath, byte[] contents)
			=> Default.OnContentUpdate?.Invoke((assemblyName, relativePath, contents));

		public void AttachToWebViewManagerIfEnabled(WebViewManager manager)
		{
			// To provide a consistent way of transporting the data across all platforms,
			// we can use the existing IJSRuntime. In turn we can get an instance of this
			// that's always attached to the currently-loaded page (if it's a Blazor page)
			// by injecting a headless root component.
			manager.AddRootComponentAsync(typeof(StaticContentUpdater), "body::after", ParameterView.Empty);
		}

		private class StaticContentUpdater : ComponentBase
		{
			protected override void BuildRenderTree(RenderTreeBuilder builder)
			{
				builder.AddContent(0, "HELLO THERE FROM THE INJECTED COMPONENT");
			}
		}
	}
}
