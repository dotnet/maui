using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace Microsoft.Maui.Hosting
{
	public class MauiMetricsBuilder : IMetricsBuilder
	{
		readonly IServiceCollection _services;

		public MauiMetricsBuilder(IServiceCollection services)
		{
			_services = services;
		}

		public IServiceCollection Services => _services;
	}
}
