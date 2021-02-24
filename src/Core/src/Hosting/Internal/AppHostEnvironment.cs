using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Maui.Hosting.Internal
{
	internal class AppHostEnvironment : IHostEnvironment
	{
		public AppHostEnvironment()
		{
		}

		public string? EnvironmentName { get; set; }
		public string? ApplicationName { get; set; }
		public string? ContentRootPath { get; set; }
		public IFileProvider? ContentRootFileProvider { get; set; }
	}
}
