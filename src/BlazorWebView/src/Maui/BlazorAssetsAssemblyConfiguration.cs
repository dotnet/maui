using System.Reflection;

namespace Microsoft.AspNetCore.Components.WebView.Maui
{
	internal class BlazorAssetsAssemblyConfiguration
	{
		public BlazorAssetsAssemblyConfiguration(Assembly assetsAssembly)
		{
			AssetsAssembly = assetsAssembly;
		}

		public Assembly AssetsAssembly { get; set; }
	}
}
